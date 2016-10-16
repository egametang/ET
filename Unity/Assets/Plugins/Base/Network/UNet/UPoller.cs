using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Base
{
	internal sealed class UPoller : IDisposable
	{
		static UPoller()
		{
			Library.Initialize();
		}

		public USocketManager USocketManager { get; }
		private readonly QueueDictionary<IntPtr, ENetEvent> connQueue = new QueueDictionary<IntPtr, ENetEvent>();

		private IntPtr host;

		// 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
		private Queue<Action> concurrentQueue = new Queue<Action>();
		private Queue<Action> localQueue;
		private readonly object lockObject = new object();

		private ENetEvent eNetEventCache;

		private TaskCompletionSource<USocket> AcceptTcs { get; set; }

		public UPoller(string hostName, ushort port)
		{
			this.USocketManager = new USocketManager();
			
			UAddress address = new UAddress(hostName, port);
			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.ENetHostCreate(ref nativeAddress,
					NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new Exception("Host creation call failed.");
			}

			NativeMethods.ENetHostCompressWithRangeCoder(this.host);
		}

		public UPoller()
		{
			this.USocketManager = new USocketManager();

			this.host = NativeMethods.ENetHostCreate(IntPtr.Zero, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new Exception("Host creation call failed.");
			}

			NativeMethods.ENetHostCompressWithRangeCoder(this.host);
		}

		public void Dispose()
		{
			if (this.host == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.ENetHostDestroy(this.host);

			this.host = IntPtr.Zero;
		}

		public IntPtr Host
		{
			get
			{
				return this.host;
			}
		}

		private ENetEvent TryGetEvent()
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
			lock (lockObject)
			{
				this.concurrentQueue.Enqueue(action);
			}
		}

		public Task<USocket> AcceptAsync()
		{
			if (this.AcceptTcs != null)
			{
				throw new Exception("do not accept twice!");
			}

			var tcs = new TaskCompletionSource<USocket>();

			// 如果有请求连接缓存的包,从缓存中取
			if (this.connQueue.Count > 0)
			{
				IntPtr ptr = this.connQueue.FirstKey;
				this.connQueue.Remove(ptr);

				USocket socket = new USocket(ptr, this);
				this.USocketManager.Add(ptr, socket);
				tcs.SetResult(socket);
			}
			else
			{
				this.AcceptTcs = tcs;
			}
			return tcs.Task;
		}

		private void OnAccepted(ENetEvent eEvent)
		{
			if (eEvent.Type == EventType.Disconnect)
			{
				this.AcceptTcs.TrySetException(new Exception("socket disconnected in accpet"));
			}
			
			USocket socket = new USocket(eEvent.Peer, this);
			this.USocketManager.Add(socket.PeerPtr, socket);
			socket.OnAccepted();

			var tcs = this.AcceptTcs;
			this.AcceptTcs = null;
			tcs.SetResult(socket);
		}

		private void OnEvents()
		{
			lock (lockObject)
			{
				localQueue = concurrentQueue;
				concurrentQueue = new Queue<Action>();
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
				ENetEvent eNetEvent = this.TryGetEvent();
				if (eNetEvent == null)
				{
					return;
				}

				switch (eNetEvent.Type)
				{
					case EventType.Connect:
						{
							// 这是一个connect peer
							if (this.USocketManager.ContainsKey(eNetEvent.Peer))
							{
								USocket uSocket = this.USocketManager[eNetEvent.Peer];
								uSocket.OnConnected();
								break;
							}

							// 这是accept peer
							if (this.AcceptTcs != null)
							{
								this.OnAccepted(eNetEvent);
								break;
							}

							// 如果server端没有acceptasync,则请求放入队列
							this.connQueue.Add(eNetEvent.Peer, eNetEvent);
							break;
						}
					case EventType.Receive:
						{
							USocket uSocket = this.USocketManager[eNetEvent.Peer];
							uSocket.OnReceived(eNetEvent);
							break;
						}
					case EventType.Disconnect:
						{
							USocket uSocket = this.USocketManager[eNetEvent.Peer];
							this.USocketManager.Remove(uSocket.PeerPtr);
							uSocket.PeerPtr = IntPtr.Zero;
							uSocket.OnDisconnect(eNetEvent);
							break;
						}
				}
			}
		}
	}
}