using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;

namespace UNet
{
	internal sealed class UPoller: IDisposable
	{
		static UPoller()
		{
			Library.Initialize();
		}

		private readonly USocketManager uSocketManager = new USocketManager();

		private readonly QueueDictionary<IntPtr, ENetEvent> connQueue =
				new QueueDictionary<IntPtr, ENetEvent>();

		private IntPtr host;

		private readonly USocket acceptor;

		// 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
		private readonly ConcurrentQueue<Action> concurrentQueue = new ConcurrentQueue<Action>();

		private readonly Queue<Action> localQueue = new Queue<Action>();

		private ENetEvent eNetEventCache;

		public UPoller(string hostName, ushort port)
		{
			this.acceptor = new USocket(IntPtr.Zero, this);
			UAddress address = new UAddress(hostName, port);
			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.ENetHostCreate(ref nativeAddress,
					NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new UException("Host creation call failed.");
			}

			NativeMethods.ENetHostCompressWithRangeCoder(this.host);
		}

		public UPoller()
		{
			this.host = NativeMethods.ENetHostCreate(IntPtr.Zero, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID,
					0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new UException("Host creation call failed.");
			}
		}

		~UPoller()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.host == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.ENetHostDestroy(this.host);

			this.host = IntPtr.Zero;
		}

		public USocketManager USocketManager
		{
			get
			{
				return this.uSocketManager;
			}
		}

		public IntPtr Host
		{
			get
			{
				return this.host;
			}
		}

		public Task<USocket> AcceptAsync()
		{
			if (this.uSocketManager.ContainsKey(IntPtr.Zero))
			{
				throw new UException("do not accept twice!");
			}

			var tcs = new TaskCompletionSource<USocket>();

			// 如果有请求连接缓存的包,从缓存中取
			if (this.connQueue.Count > 0)
			{
				IntPtr ptr = this.connQueue.FirstKey;
				this.connQueue.Remove(ptr);

				USocket socket = new USocket(ptr, this);
				this.uSocketManager.Add(ptr, socket);
				tcs.TrySetResult(socket);
			}
			else
			{
				this.uSocketManager.Add(this.acceptor.PeerPtr, this.acceptor);
				this.acceptor.Connected = eEvent =>
				{
					if (eEvent.Type == EventType.Disconnect)
					{
						tcs.TrySetException(new UException("socket disconnected in accpet"));
					}

					this.uSocketManager.Remove(IntPtr.Zero);
					USocket socket = new USocket(eEvent.Peer, this);
					this.uSocketManager.Add(socket.PeerPtr, socket);
					tcs.TrySetResult(socket);
				};
			}
			return tcs.Task;
		}

		private ENetEvent GetEvent()
		{
			if (this.eNetEventCache == null)
			{
				this.eNetEventCache = new ENetEvent();
			}
			if (NativeMethods.ENetHostCheckEvents(this.host, this.eNetEventCache) <= 0)
			{
				return null;
			}
			ENetEvent eNetEvent = this.eNetEventCache;
			this.eNetEventCache = null;
			return eNetEvent;
		}

		public void Flush()
		{
			NativeMethods.ENetHostFlush(this.host);
		}

		public void Add(Action action)
		{
			this.concurrentQueue.Enqueue(action);
		}

		private void OnEvents()
		{
			while (true)
			{
				Action action;
				if (!this.concurrentQueue.TryDequeue(out action))
				{
					break;
				}
				this.localQueue.Enqueue(action);
			}

			while (this.localQueue.Count > 0)
			{
				Action a = this.localQueue.Dequeue();
				a();
			}
		}

		private int Service()
		{
			int ret = NativeMethods.ENetHostService(this.host, null, 0);
			return ret;
		}

		public void Update()
		{
			this.OnEvents();

			if (this.Service() < 0)
			{
				return;
			}

			while (true)
			{
				ENetEvent eNetEvent = this.GetEvent();
				if (eNetEvent == null)
				{
					return;
				}

				switch (eNetEvent.Type)
				{
					case EventType.Connect:
					{
						// 这是一个connect peer
						if (this.uSocketManager.ContainsKey(eNetEvent.Peer))
						{
							USocket uSocket = this.uSocketManager[eNetEvent.Peer];
							uSocket.OnConnected(eNetEvent);
							break;
						}

						// 这是accept peer
						if (this.uSocketManager.ContainsKey(IntPtr.Zero))
						{
							USocket uSocket = this.uSocketManager[IntPtr.Zero];
							uSocket.OnConnected(eNetEvent);
							break;
						}

						// 如果server端没有acceptasync,则请求放入队列
						this.connQueue.Add(eNetEvent.Peer, eNetEvent);
						break;
					}
					case EventType.Receive:
					{
						USocket uSocket = this.uSocketManager[eNetEvent.Peer];
						uSocket.OnReceived(eNetEvent);
						break;
					}
					case EventType.Disconnect:
					{
						// 如果链接还在缓存中，则删除
						if (this.connQueue.Remove(eNetEvent.Peer))
						{
							break;
						}

						// 链接已经被应用层接收
						USocket uSocket = this.uSocketManager[eNetEvent.Peer];
						this.uSocketManager.Remove(eNetEvent.Peer);

						// 等待的task将抛出异常
						if (uSocket.Connected != null)
						{
							uSocket.OnConnected(eNetEvent);
							break;
						}
						if (uSocket.Received != null)
						{
							uSocket.OnReceived(eNetEvent);
							break;
						}
						if (uSocket.Disconnect != null)
						{
							uSocket.OnDisconnect(eNetEvent);
							break;
						}
						break;
					}
				}
			}
		}
	}
}