using System;

namespace ENet
{
	public abstract class Host: IDisposable
	{
		static Host()
		{
			Library.Initialize();
		}

		private readonly PeersManager peersManager = new PeersManager();

		protected PeersManager PeersManager
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

		~Host()
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

		protected void EnableCrc()
		{
			NativeMethods.enet_enable_crc(this.host);
		}

		protected static void CheckChannelLimit(uint channelLimit)
		{
			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException("channelLimit");
			}
		}

		protected int CheckEvents(out Event e)
		{
			var enetEv = new ENetEvent();
			int ret = NativeMethods.enet_host_check_events(this.host, enetEv);
			e = new Event(enetEv);
			return ret;
		}

		protected int Service(int timeout)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			return NativeMethods.enet_host_service(this.host, null, (uint) timeout);
		}

		public void Broadcast(byte channelID, ref Packet packet)
		{
			NativeMethods.enet_host_broadcast(this.host, channelID, packet.PacketPtr);
		}

		public void CompressWithRangeEncoder()
		{
			NativeMethods.enet_host_compress_with_range_encoder(this.host);
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

		protected void OnEvents()
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
	}
}