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
	public unsafe struct Peer
	{
		private Native.ENetPeer* peer;

		public Peer(Native.ENetPeer* peer)
		{
			this.peer = peer;
		}

		private void CheckCreated()
		{
			if (this.peer == null)
			{
				throw new InvalidOperationException("No native peer.");
			}
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			this.CheckCreated();
			Native.enet_peer_throttle_configure(this.peer, interval, acceleration, deceleration);
		}

		public void Disconnect(uint data)
		{
			this.CheckCreated();
			Native.enet_peer_disconnect(this.peer, data);
		}

		public void DisconnectLater(uint data)
		{
			this.CheckCreated();
			Native.enet_peer_disconnect_later(this.peer, data);
		}

		public void DisconnectNow(uint data)
		{
			this.CheckCreated();
			Native.enet_peer_disconnect_now(this.peer, data);
		}

		public void Ping()
		{
			this.CheckCreated();
			Native.enet_peer_ping(this.peer);
		}

		public void Reset()
		{
			this.CheckCreated();
			Native.enet_peer_reset(this.peer);
		}

		public bool Receive(out byte channelID, out Packet packet)
		{
			this.CheckCreated();
			Native.ENetPacket* nativePacket = Native.enet_peer_receive(this.peer, out channelID);
			if (nativePacket == null)
			{
				packet = new Packet();
				return false;
			}
			packet = new Packet(nativePacket);
			return true;
		}

		public void Send(byte channelID, byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.Send(channelID, data, 0, data.Length);
		}

		public void Send(byte channelID, byte[] data, int offset, int length)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			using (var packet = new Packet())
			{
				packet.Create(data, offset, length);
				this.Send(channelID, packet);
			}
		}

		public void Send(byte channelID, Packet packet)
		{
			this.CheckCreated();
			packet.CheckCreated();
			Native.enet_peer_send(this.peer, channelID, packet.NativeData);
		}

		public bool IsSet
		{
			get
			{
				return this.peer != null;
			}
		}

		public Native.ENetPeer* NativeData
		{
			get
			{
				return this.peer;
			}
			set
			{
				this.peer = value;
			}
		}

		public PeerState State
		{
			get
			{
				return this.IsSet? this.peer->state : PeerState.Uninitialized;
			}
		}

		public IntPtr UserData
		{
			get
			{
				this.CheckCreated();
				return this.peer->data;
			}
			set
			{
				this.CheckCreated();
				this.peer->data = value;
			}
		}
	}
}