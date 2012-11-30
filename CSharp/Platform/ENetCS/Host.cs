using System;
using System.Threading.Tasks;

namespace ENet
{
	public sealed class Host: IDisposable
	{
		private readonly PeersManager peersManager = new PeersManager();

		public PeersManager PeersManager
		{
			get
			{
				return this.peersManager;
			}
		}

		private IntPtr host;
		private Action<Event> acceptHandler;
		private readonly object eventsLock = new object();
		private Action events;

		public Host(Address address, uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 
				uint channelLimit = 0, uint incomingBandwidth = 0, 
				uint outgoingBandwidth = 0, bool enableCrc = true)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			ENetAddress nativeAddress = address.Struct;
			this.host = NativeMethods.enet_host_create(ref nativeAddress, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException(0, "Host creation call failed.");
			}

			if (enableCrc)
			{
				NativeMethods.enet_enable_crc(this.host);
			}
		}

		public Host(uint peerLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID, 
				uint channelLimit = 0, uint incomingBandwidth = 0,
				uint outgoingBandwidth = 0, bool enableCrc = true)
		{
			if (peerLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			this.host = NativeMethods.enet_host_create(IntPtr.Zero, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException(0, "Host creation call failed.");
			}

			if (enableCrc)
			{
				NativeMethods.enet_enable_crc(this.host);
			}
		}

		~Host()
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

			NativeMethods.enet_host_destroy(this.host);

			this.host = IntPtr.Zero;
		}

		private static void CheckChannelLimit(uint channelLimit)
		{
			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException("channelLimit");
			}
		}

		private int CheckEvents(out Event e)
		{
			var enetEv = new ENetEvent();
			int ret = NativeMethods.enet_host_check_events(this.host, enetEv);
			e = new Event(this, enetEv);
			return ret;
		}

		private int Service(int timeout)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			return NativeMethods.enet_host_service(this.host, null, (uint) timeout);
		}

		public void Broadcast(byte channelID, ref Packet packet)
		{
			NativeMethods.enet_host_broadcast(this.host, channelID, packet.NativePtr);
		}

		public void CompressWithRangeEncoder()
		{
			NativeMethods.enet_host_compress_with_range_encoder(this.host);
		}

		public void DoNotCompress()
		{
			NativeMethods.enet_host_compress(this.host, IntPtr.Zero);
		}

		public Task<Peer> ConnectAsync(
				Address address, uint channelLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT, uint data = 0)
		{
			CheckChannelLimit(channelLimit);

			var tcs = new TaskCompletionSource<Peer>();
			ENetAddress nativeAddress = address.Struct;
			IntPtr p = NativeMethods.enet_host_connect(this.host, ref nativeAddress, channelLimit, data);
			if (p == IntPtr.Zero)
			{
				throw new ENetException(0, "Host connect call failed.");
			}
			var peer = new Peer(this, p);
			this.PeersManager[p].PeerEvent.Connected += e => tcs.TrySetResult(peer);
			return tcs.Task;
		}

		public Task<Peer> AcceptAsync()
		{
			if (acceptHandler != null)
			{
				throw new ENetException(0, "don't accept twice, when last accept not return!");
			}
			var tcs = new TaskCompletionSource<Peer>();
			acceptHandler += e => tcs.TrySetResult(e.Peer);
			return tcs.Task;
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
			CheckChannelLimit(channelLimit);
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

		private void OnExecuteEvents()
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

		public void Run()
		{
			this.OnExecuteEvents();

			if (this.Service(0) < 0)
			{
				return;
			}

			Event ev;
			while (this.CheckEvents(out ev) > 0)
			{
				switch (ev.Type)
				{
					case EventType.Connect:
					{
						// 如果PeersManager包含了peer,则这次是connect事件
						// 反之是accept事件
						if (this.PeersManager.ContainsKey(ev.Ev.peer))
						{
							ev.Peer.PeerEvent.OnConnected(ev);
						}
						else
						{
							if (acceptHandler != null)
							{
								acceptHandler(ev);
								acceptHandler = null;
							}
						}
						break;
					}
					case EventType.Receive:
					{
						ev.Peer.PeerEvent.OnReceived(ev);
						break;
					}
					case EventType.Disconnect:
					{
						ev.Peer.PeerEvent.OnDisconnect(ev);
						break;
					}
				}
			}
		}
	}
}