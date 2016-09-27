using System;
using System.Collections.Generic;

namespace Base
{
	internal sealed class UPoller : IDisposable
	{
		static UPoller()
		{
			Library.Initialize();
		}

		private IntPtr host;

		// 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
		private Queue<Action> concurrentQueue = new Queue<Action>();

		private Queue<Action> localQueue;

		private ENetEvent eNetEventCache;

		private readonly object lockObject = new object();

		public UPoller()
		{
			this.USocketManager = new USocketManager();
			this.host = NativeMethods.ENetHostCreate(IntPtr.Zero, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new GameException("Host creation call failed.");
			}
		}

		~UPoller()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
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

		public USocketManager USocketManager { get; }

		public IntPtr Host
		{
			get
			{
				return this.host;
			}
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
							if (this.USocketManager.ContainsKey(eNetEvent.Peer))
							{
								USocket uSocket = this.USocketManager[eNetEvent.Peer];
								uSocket.OnConnected(eNetEvent);
							}
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