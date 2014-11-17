using System;
using System.Collections.Generic;

namespace ENet
{
	public sealed class EService: IDisposable
	{
		static EService()
		{
			Library.Initialize();
		}

		private readonly PeersManager peersManager = new PeersManager();
		private readonly LinkedList<EEvent> connEEvents = new LinkedList<EEvent>();

		internal PeersManager PeersManager
		{
			get
			{
				return this.peersManager;
			}
		}

		internal LinkedList<EEvent> ConnEEvents
		{
			get
			{
				return this.connEEvents;
			}
		}

		private IntPtr host;
		private bool isRunning = true;
		private readonly object eventsLock = new object();
		private Action events;

		public EService(
				string hostName, ushort port, uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID,
				uint channelLimit = 0, uint incomingBandwidth = 0, uint outgoingBandwidth = 0)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException(string.Format("peerLimit: {0}", peerLimit));
			}

			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException(string.Format("channelLimit: {0}", channelLimit));
			}

			var address = new Address { HostName = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.EnetHostCreate(ref nativeAddress, peerLimit, channelLimit,
					incomingBandwidth, outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new EException("Host creation call failed.");
			}
		}

		public EService(
				uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, uint channelLimit = 0,
				uint incomingBandwidth = 0, uint outgoingBandwidth = 0)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException(string.Format("peerLimit: {0}", peerLimit));
			}

			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException(string.Format("channelLimit: {0}", channelLimit));
			}

			this.host = NativeMethods.EnetHostCreate(IntPtr.Zero, peerLimit, channelLimit, incomingBandwidth,
					outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new EException("Host creation call failed.");
			}
		}

		~EService()
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

		public IntPtr HostPtr
		{
			get
			{
				return this.host;
			}
		}

		public void EnableCrc()
		{
			NativeMethods.EnetEnableCrc(this.host);
		}

		private EEvent GetEvent()
		{
			var enetEv = new ENetEvent();
			int ret = NativeMethods.EnetHostCheckEvents(this.host, enetEv);
			if (ret <= 0)
			{
				return null;
			}
			var e = new EEvent(enetEv);
			return e;
		}

		public void CompressWithRangeCoder()
		{
			NativeMethods.EnetHostCompressWithRangeCoder(this.host);
		}

		public void DoNotCompress()
		{
			NativeMethods.EnetHostCompress(this.host, IntPtr.Zero);
		}

		public void Flush()
		{
			NativeMethods.EnetHostFlush(this.host);
		}

		public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth)
		{
			NativeMethods.EnetHostBandwidthLimit(this.host, incomingBandwidth, outgoingBandwidth);
		}

		public void SetChannelLimit(uint channelLimit)
		{
			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException(string.Format("channelLimit: {0}", channelLimit));
			}
			NativeMethods.EnetHostChannelLimit(this.host, channelLimit);
		}

		public event Action Events
		{
			add
			{
				lock (this.eventsLock)
				{
					this.events += value;
				}
			}
			remove
			{
				lock (this.eventsLock)
				{
					this.events -= value;
				}
			}
		}

		public void OnEvents()
		{
			Action local = null;
			lock (this.eventsLock)
			{
				if (this.events == null)
				{
					return;
				}
				local = this.events;
				this.events = null;
			}
			local();
		}

		public void Stop()
		{
			this.isRunning = false;
		}

		private int Service(int timeout)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format("timeout: {0}", timeout));
			}
			return NativeMethods.EnetHostService(this.host, null, (uint) timeout);
		}

		public void RunOnce(int timeout = 0)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format("timeout: {0}", timeout));
			}

			this.OnEvents();

			if (this.Service(timeout) < 0)
			{
				return;
			}

			while (true)
			{
				EEvent eEvent = this.GetEvent();
				if (eEvent == null)
				{
					return;
				}

				switch (eEvent.Type)
				{
					case EventType.Connect:
					{
						// 这是一个connect peer
						if (this.PeersManager.ContainsKey(eEvent.PeerPtr))
						{
							ESocket eSocket = this.PeersManager[eEvent.PeerPtr];
							eSocket.OnConnected(eEvent);
						}
								// accept peer
						else
						{
							// 如果server端没有acceptasync,则请求放入队列
							if (!this.PeersManager.ContainsKey(IntPtr.Zero))
							{
								this.connEEvents.AddLast(eEvent);
							}
							else
							{
								ESocket eSocket = this.PeersManager[IntPtr.Zero];
								eSocket.OnConnected(eEvent);
							}
						}
						break;
					}
					case EventType.Receive:
					{
						ESocket eSocket = this.PeersManager[eEvent.PeerPtr];
						eSocket.OnReceived(eEvent);
						break;
					}
					case EventType.Disconnect:
					{
						// 如果链接还在缓存中，则删除
						foreach (EEvent connEEvent in this.connEEvents)
						{
							if (connEEvent.PeerPtr != eEvent.PeerPtr)
							{
								continue;
							}
							this.connEEvents.Remove(connEEvent);
							return;
						}

						// 链接已经被应用层接收
						eEvent.EventState = EventState.DISCONNECTED;
						ESocket eSocket = this.PeersManager[eEvent.PeerPtr];
						this.PeersManager.Remove(eEvent.PeerPtr);

						// 等待的task将抛出异常
						if (eSocket.Connected != null)
						{
							eSocket.OnConnected(eEvent);
						}
						else if (eSocket.Received != null)
						{
							eSocket.OnReceived(eEvent);
						}
						else if (eSocket.Disconnect != null)
						{
							eSocket.OnDisconnect(eEvent);
						}

						eSocket.OnError(ErrorCode.ClientDisconnect);
						break;
					}
				}
			}
		}

		public void Start(int timeout = 0)
		{
			while (this.isRunning)
			{
				this.RunOnce(timeout);
			}
		}
	}
}