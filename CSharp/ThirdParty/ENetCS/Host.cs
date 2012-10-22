using System;
using System.Threading.Tasks;

namespace ENet
{
	public sealed class Host : IDisposable
	{
		private IntPtr host;
		private readonly object eventsLock = new object();
		private Action events;

		public Host(Address address, uint peerLimit = Native.ENET_PROTOCOL_MAXIMUM_PEER_ID, 
			uint channelLimit = 0, uint incomingBandwidth = 0,
			uint outgoingBandwidth = 0, bool enableCrc = true)
		{
			if (peerLimit > Native.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			ENetAddress nativeAddress = address.Struct;
			this.host = Native.enet_host_create(
				ref nativeAddress, peerLimit, channelLimit, incomingBandwidth,
				outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException(0, "Host creation call failed.");
			}

			if (enableCrc)
			{
				Native.enet_enable_crc(host);
			}
		}

		public Host(uint peerLimit = Native.ENET_PROTOCOL_MAXIMUM_PEER_ID, uint channelLimit = 0,
				uint incomingBandwidth = 0, uint outgoingBandwidth = 0, bool enableCrc = true)
		{
			if (peerLimit > Native.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			this.host = Native.enet_host_create(
				IntPtr.Zero, peerLimit, channelLimit, incomingBandwidth,
				outgoingBandwidth);

			if (this.host == IntPtr.Zero)
			{
				throw new ENetException(0, "Host creation call failed.");
			}

			if (enableCrc)
			{
				Native.enet_enable_crc(host);
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
			Native.enet_host_destroy(this.host);
			this.host = IntPtr.Zero;
		}

		private static void CheckChannelLimit(uint channelLimit)
		{
			if (channelLimit > Native.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException("channelLimit");
			}
		}

		private int CheckEvents(out Event e)
		{
			var enetEv = new ENetEvent();
			int ret = Native.enet_host_check_events(this.host, enetEv);
			e = new Event(enetEv);
			return ret;
		}

		private int Service(int timeout)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			return Native.enet_host_service(this.host, null, (uint)timeout);
		}

		public void Broadcast(byte channelID, ref Packet packet)
		{
			Native.enet_host_broadcast(this.host, channelID, packet.NativePtr);
		}

		public void CompressWithRangeEncoder()
		{
			Native.enet_host_compress_with_range_encoder(this.host);
		}

		public void DoNotCompress()
		{
			Native.enet_host_compress(this.host, IntPtr.Zero);
		}

		public Task<Peer> ConnectAsync(
			Address address, uint channelLimit = Native.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT, 
			uint data = 0)
		{
			CheckChannelLimit(channelLimit);

			var tcs = new TaskCompletionSource<Peer>();
			ENetAddress nativeAddress = address.Struct;
			IntPtr p = Native.enet_host_connect(this.host, ref nativeAddress, channelLimit, data);
			if (p == IntPtr.Zero)
			{
				throw new ENetException(0, "Host connect call failed.");
			}
			var peer = new Peer(p);
			Peer.PeerEventsManager[p].Connected += e => tcs.TrySetResult(peer);
			return tcs.Task;
		}

		public void Flush()
		{
			Native.enet_host_flush(this.host);
		}

		public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth)
		{
			Native.enet_host_bandwidth_limit(this.host, incomingBandwidth, outgoingBandwidth);
		}

		public void SetChannelLimit(uint channelLimit)
		{
			CheckChannelLimit(channelLimit);
			Native.enet_host_channel_limit(this.host, channelLimit);
		}

		public event Action Events
		{
			add
			{
				lock (eventsLock)
				{
					events += value;
				}
			}
			remove
			{
				lock (eventsLock)
				{
					events -= value;
				}
			}
		}

		private void OnExecuteEvents()
		{
			Action local = null;
			lock (eventsLock)
			{
				if (events == null)
				{
					return;
				}
				local = events;
				events = null;
			}
			local();
		}

		public void Run()
		{
			// 处理其它线程扔过来的事件
			OnExecuteEvents();

			if (this.Service(0) < 0)
			{
				return;
			}

			Event e;
			while (this.CheckEvents(out e) > 0)
			{
				switch (e.Type)
				{
					case EventType.Connect:
					{
						Peer.PeerEventsManager.OnConnected(e.Ev.peer, e);
						break;
					}
					case EventType.Receive:
					{
						Peer.PeerEventsManager.OnReceived(e.Ev.peer, e);
						break;
					}
					case EventType.Disconnect:
					{
						Peer.PeerEventsManager.OnDisconnect(e.Ev.peer, e);
						break;
					}
				}
			}
		}
	}
}