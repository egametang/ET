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
		private readonly QueueDictionary<IntPtr, ENetEvent> connQueue = new QueueDictionary<IntPtr, ENetEvent>();

		private IntPtr host;

		private readonly USocket acceptor = new USocket(IntPtr.Zero);

		private readonly BlockingCollection<Action> blockingCollection = new BlockingCollection<Action>();

		public UPoller(string hostName, ushort port)
		{
			UAddress address = new UAddress { Host = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.EnetHostCreate(
				ref nativeAddress, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new UException("Host creation call failed.");
			}

			NativeMethods.EnetHostCompressWithRangeCoder(this.host);
		}

		public UPoller()
		{
			this.host = NativeMethods.EnetHostCreate(
				IntPtr.Zero, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

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

			NativeMethods.EnetHostDestroy(this.host);

			this.host = IntPtr.Zero;
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

				USocket socket = new USocket(ptr);
				this.uSocketManager.Add(ptr, socket);
				tcs.TrySetResult(socket);
			}
			else
			{
				this.uSocketManager.Add(acceptor.PeerPtr, acceptor);
				acceptor.Connected = eEvent =>
				{
					if (eEvent.Type == EventType.Disconnect)
					{
						tcs.TrySetException(new UException("socket disconnected in accpet"));
					}

					this.uSocketManager.Remove(IntPtr.Zero);
					USocket socket = new USocket(eEvent.Peer);
					this.uSocketManager.Add(socket.PeerPtr, socket);
					tcs.TrySetResult(socket);
				};
			}
			return tcs.Task;
		}

		public Task<USocket> ConnectAsync(string hostName, ushort port)
		{
			var tcs = new TaskCompletionSource<USocket>();
			UAddress address = new UAddress { Host = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			
			IntPtr ptr = NativeMethods.EnetHostConnect(
				this.host, ref nativeAddress, NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT, 0);
			USocket socket = new USocket(ptr);
			if (socket.PeerPtr == IntPtr.Zero)
			{
				throw new UException("host connect call failed.");
			}
			this.uSocketManager.Add(socket.PeerPtr, socket);
			socket.Connected = eEvent =>
			{
				if (eEvent.Type == EventType.Disconnect)
				{
					tcs.TrySetException(new UException("socket disconnected in connect"));
				}
				tcs.TrySetResult(socket);
			};
			return tcs.Task;
		}

		private ENetEvent GetEvent()
		{
			ENetEvent eNetEvent = new ENetEvent();
			if (NativeMethods.EnetHostCheckEvents(this.host, eNetEvent) <= 0)
			{
				return null;
			}
			return eNetEvent;
		}

		public void Flush()
		{
			NativeMethods.EnetHostFlush(this.host);
		}

		public void Add(Action action)
		{
			blockingCollection.Add(action);
		}

		private void OnEvents(int timeout)
		{
			// 处理读写线程的回调
			Action action;
			if (!this.blockingCollection.TryTake(out action, timeout))
			{
				return;
			}

			var queue = new Queue<Action>();
			queue.Enqueue(action);

			while (this.blockingCollection.TryTake(out action, 0))
			{
				queue.Enqueue(action);
			}

			while (queue.Count > 0)
			{
				Action a = queue.Dequeue();
				a();
			}
		}

		private int Service()
		{
			return NativeMethods.EnetHostService(this.host, null, 0);
		}

		public void RunOnce(int timeout = 0)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format("timeout: {0}", timeout));
			}

			this.OnEvents(timeout);

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

		public void Run(int timeout = 0)
		{
			while (true)
			{
				this.RunOnce(timeout);
			}
		}
	}
}