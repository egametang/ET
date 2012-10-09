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
	public unsafe class Peer : IDisposable
	{
		private Native.ENetPeer* peer;

		public Peer(Native.ENetPeer* peer)
		{
			if (peer == null)
			{
				throw new InvalidOperationException("No native peer.");
			}
			this.peer = peer;
		}

		~Peer()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (this.peer == null)
			{
				return;
			}

			if (disposing)
			{
				Native.enet_peer_reset(this.peer);
				this.peer = null;
			}
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			Native.enet_peer_throttle_configure(this.peer, interval, acceleration, deceleration);
		}

		public void Disconnect(uint data)
		{
			Native.enet_peer_disconnect(this.peer, data);
		}

		public void DisconnectLater(uint data)
		{
			Native.enet_peer_disconnect_later(this.peer, data);
		}

		public void DisconnectNow(uint data)
		{
			Native.enet_peer_disconnect_now(this.peer, data);
		}

		public void Ping()
		{
			Native.enet_peer_ping(this.peer);
		}

		public bool Receive(out byte channelID, out Packet packet)
		{
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
			this.Send(channelID, data, 0, data.Length);
		}

		public void Send(byte channelID, byte[] data, int offset, int length)
		{
			using (var packet = new Packet(data, offset, length))
			{
				this.Send(channelID, packet);
			}
		}

		public void Send(byte channelID, Packet packet)
		{
			Native.enet_peer_send(this.peer, channelID, packet.NativeData);
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
				return this.peer->state;
			}
		}

		public IntPtr Data
		{
			get
			{
				return this.peer->data;
			}
			set
			{
				this.peer->data = value;
			}
		}
	}
}