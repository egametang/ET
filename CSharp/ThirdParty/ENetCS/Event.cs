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

using Log;

namespace ENet
{
	public unsafe struct Event
	{
		private readonly Host host;
		private Native.ENetEvent e;

		public Event(Host host, Native.ENetEvent e)
		{
			this.e = e;
			this.host = host;
		}

		public byte ChannelID
		{
			get
			{
				return this.e.channelID;
			}
		}

		public uint Data
		{
			get
			{
				return this.e.data;
			}
		}

		public Native.ENetEvent NativeData
		{
			get
			{
				return this.e;
			}
			set
			{
				this.e = value;
			}
		}

		public Packet Packet
		{
			get
			{
				if (this.e.packet == null)
				{
					return null;
				}
				return new Packet(this.e.packet);
			}
		}

		public Peer Peer
		{
			get
			{
				if (this.e.peer == null)
				{
					return null;
				}
				var data = (int)e.peer->data;
				if (!this.host.Peers.ContainsKey(data))
				{
					return null;
				}
				return this.host.Peers[data];
			}
		}

		public EventType Type
		{
			get
			{
				return this.e.type;
			}
		}
	}
}