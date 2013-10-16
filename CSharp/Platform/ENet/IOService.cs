using System;

namespace ENet
{
	public class IOService: IDisposable
	{
		static IOService()
		{
			Library.Initialize();
		}

		private readonly PeersManager peersManager = new PeersManager();

		public PeersManager PeersManager
		{
			get
			{
				return this.peersManager;
			}
		}

		protected IntPtr host;
		protected bool isRunning = true;
		private readonly object eventsLock = new object();
		private Action events;

		public IOService(string hostName, ushort port, 
			uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID,
			uint channelLimit = 0, 
			uint incomingBandwidth = 0, 
			uint outgoingBandwidth = 0)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}

			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException(string.Format("channelLimit: {0}", channelLimit));
			}

			var address = new Address { HostName = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.enet_host_create(
				ref nativeAddress, peerLimit, channelLimit, incomingBandwidth, 
				outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException("Host creation call failed.");
			}
		}

		public IOService(
			uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 
			uint channelLimit = 0,
			uint incomingBandwidth = 0, 
			uint outgoingBandwidth = 0)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}

			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException(string.Format("channelLimit: {0}", channelLimit));
			}

			this.host = NativeMethods.enet_host_create(
				IntPtr.Zero, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException("Host creation call failed.");
			}
		}

		~IOService()
		{
			this.Dispose(false);
		}

		public virtual void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (this.host == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.enet_host_destroy(this.host);

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
			NativeMethods.enet_enable_crc(this.host);
		}

		private Event GetEvent()
		{
			var enetEv = new ENetEvent();
			int ret = NativeMethods.enet_host_check_events(this.host, enetEv);
			if (ret <= 0)
			{
				return null;
			}
			var e = new Event(enetEv);
			return e;
		}

		public void CompressWithRangeCoder()
		{
			NativeMethods.enet_host_compress_with_range_coder(this.host);
		}

		public void DoNotCompress()
		{
			NativeMethods.enet_host_compress(this.host, IntPtr.Zero);
		}

		public void Flush()
		{
			NativeMethods.enet_host_flush(this.host);
		}

		public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth)
		{
			NativeMethods.enet_host_bandwidth_limit(this.host, incomingBandwidth, outgoingBandwidth);
		}

		public void SetChannelLimit(uint channelLimit)
		{
			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException(string.Format("channelLimit: {0}", channelLimit));
			}
			NativeMethods.enet_host_channel_limit(this.host, channelLimit);
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
			return NativeMethods.enet_host_service(this.host, null, (uint)timeout);
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
				Event ev = this.GetEvent();
				if (ev == null)
				{
					return;
				}

				switch (ev.Type)
				{
					case EventType.Connect:
					{
						// 这是一个connect peer,否则是一个accept peer
						if (this.PeersManager.ContainsKey(ev.PeerPtr))
						{
							var peer = this.PeersManager[ev.PeerPtr];
							peer.ESocketEvent.OnConnected(ev);
							peer.ESocketEvent.Connected = null;
						}
						else
						{
							var peer = this.PeersManager[IntPtr.Zero];

							this.PeersManager.Remove(IntPtr.Zero);

							peer.PeerPtr = ev.PeerPtr;
							this.PeersManager.Add(peer.PeerPtr, peer);

							peer.ESocketEvent.OnConnected(ev);
							peer.ESocketEvent.Connected = null;
						}
						break;
					}
					case EventType.Receive:
					{
						var peer = this.PeersManager[ev.PeerPtr];
						peer.ESocketEvent.OnReceived(ev);
						peer.ESocketEvent.Received = null;
						break;
					}
					case EventType.Disconnect:
					{
						ev.EventState = EventState.DISCONNECTED;

						var peer = this.PeersManager[ev.PeerPtr];
						ESocketEvent peerEvent = peer.ESocketEvent;

						this.PeersManager.Remove(peer.PeerPtr);
						// enet_peer_disconnect会reset Peer,这里设置为0,防止再次Dispose
						peer.PeerPtr = IntPtr.Zero;

						if (peerEvent.Received != null)
						{
							peerEvent.OnReceived(ev);
						}
						else
						{
							peerEvent.OnDisconnect(ev);
						}
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