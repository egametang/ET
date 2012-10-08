#region License

/*
ENet for C#
Copyright (c) 2011 James F. Bellinger <jfb@zer7.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

#endregion

using System;

namespace ENet
{
	public sealed unsafe class Host : IDisposable
	{
		private Native.ENetHost* host;

		public Host(ushort port, uint peerLimit):
			this(new Address { Port = port }, peerLimit)
		{
		}

		public Host(Address? address, uint peerLimit):
			this(address, peerLimit, 0)
		{
		}

		public Host(Address? address, uint peerLimit, uint channelLimit):
			this(address, peerLimit, channelLimit, 0, 0)
		{
		}

		public Host(Address? address, uint peerLimit, uint channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
		{
			if (this.host != null)
			{
				throw new InvalidOperationException("Already created.");
			}
			if (peerLimit > Native.ENET_PROTOCOL_MAXIMUM_PEER_ID)
			{
				throw new ArgumentOutOfRangeException("peerLimit");
			}
			CheckChannelLimit(channelLimit);

			if (address != null)
			{
				Native.ENetAddress nativeAddress = address.Value.NativeData;
				this.host = Native.enet_host_create(
					ref nativeAddress, peerLimit, channelLimit, incomingBandwidth,
					outgoingBandwidth);
			}
			else
			{
				this.host = Native.enet_host_create(
					null, peerLimit, channelLimit, incomingBandwidth,
					outgoingBandwidth);
			}
			if (this.host == null)
			{
				throw new ENetException(0, "Host creation call failed.");
			}
		}

		~Host()
		{
			this.Dispose();
		}

		private static void CheckChannelLimit(uint channelLimit)
		{
			if (channelLimit > Native.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException("channelLimit");
			}
		}

		private void CheckCreated()
		{
			if (this.host == null)
			{
				throw new InvalidOperationException("Not created.");
			}
		}

		public void Dispose()
		{
			if (this.host == null)
			{
				return;
			}
			Native.enet_host_destroy(this.host);
			this.host = null;
		}

		public void Broadcast(byte channelID, ref Packet packet)
		{
			this.CheckCreated();
			packet.CheckCreated();
			Native.enet_host_broadcast(this.host, channelID, packet.NativeData);
			packet.NativeData = null; // Broadcast automatically clears this.
		}

		public void CompressWithRangeEncoder()
		{
			this.CheckCreated();
			Native.enet_host_compress_with_range_encoder(this.host);
		}

		public void DoNotCompress()
		{
			this.CheckCreated();
			Native.enet_host_compress(this.host, null);
		}

		public int CheckEvents(out Event e)
		{
			this.CheckCreated();

			Native.ENetEvent nativeEvent;
			int ret = Native.enet_host_check_events(this.host, out nativeEvent);
			e = new Event(nativeEvent);
			return ret;
		}

		public Peer Connect(Address address, uint channelLimit, uint data)
		{
			this.CheckCreated();
			CheckChannelLimit(channelLimit);

			Native.ENetAddress nativeAddress = address.NativeData;
			Native.ENetPeer* p = Native.enet_host_connect(this.host, ref nativeAddress, channelLimit, data);
			if (p == null)
			{
				throw new ENetException(0, "Host connect call failed.");
			}
			var peer = new Peer(p);
			return peer;
		}

		public void Flush()
		{
			this.CheckCreated();
			Native.enet_host_flush(this.host);
		}

		public int Service(int timeout)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			this.CheckCreated();
			return Native.enet_host_service(this.host, null, (uint) timeout);
		}

		public int Service(int timeout, out Event e)
		{
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			this.CheckCreated();
			Native.ENetEvent nativeEvent;

			int ret = Native.enet_host_service(this.host, out nativeEvent, (uint) timeout);
			e = new Event(nativeEvent);
			return ret;
		}

		public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth)
		{
			this.CheckCreated();
			Native.enet_host_bandwidth_limit(this.host, incomingBandwidth, outgoingBandwidth);
		}

		public void SetChannelLimit(uint channelLimit)
		{
			CheckChannelLimit(channelLimit);
			this.CheckCreated();
			Native.enet_host_channel_limit(this.host, channelLimit);
		}
	}
}