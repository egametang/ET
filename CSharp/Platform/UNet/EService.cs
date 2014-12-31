using System;
using System.Collections.Generic;

namespace UNet
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
		private readonly object eventsLock = new object();
		private Action events;

		public EService(string hostName, ushort port)
		{
			Address address = new Address { HostName = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.EnetHostCreate(
				ref nativeAddress, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

			if (this.host == IntPtr.Zero)
			{
				throw new EException("Host creation call failed.");
			}
		}

		public EService()
		{
			this.host = NativeMethods.EnetHostCreate(
				IntPtr.Zero, NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 0, 0, 0);

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

		private EEvent GetEvent()
		{
			ENetEvent enetEv = new ENetEvent();
			int ret = NativeMethods.EnetHostCheckEvents(this.host, enetEv);
			if (ret <= 0)
			{
				return null;
			}
			EEvent e = new EEvent(enetEv);
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
							USocket uSocket = this.PeersManager[eEvent.PeerPtr];
							uSocket.OnConnected(eEvent);
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
								USocket uSocket = this.PeersManager[IntPtr.Zero];
								uSocket.OnConnected(eEvent);
							}
						}
						break;
					}
					case EventType.Receive:
					{
						USocket uSocket = this.PeersManager[eEvent.PeerPtr];
						uSocket.OnReceived(eEvent);
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
						USocket uSocket = this.PeersManager[eEvent.PeerPtr];
						this.PeersManager.Remove(eEvent.PeerPtr);

						// 等待的task将抛出异常
						if (uSocket.Connected != null)
						{
							uSocket.OnConnected(eEvent);
						}
						else if (uSocket.Received != null)
						{
							uSocket.OnReceived(eEvent);
						}
						else if (uSocket.Disconnect != null)
						{
							uSocket.OnDisconnect(eEvent);
						}

						uSocket.OnError(ErrorCode.ClientDisconnect);
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