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
using System.Threading.Tasks;

namespace ENet
{
	public unsafe class ENetPeer : IDisposable
	{
		private readonly ENetHost host;
		private Native.ENetPeer* peer;
		private Action<ENetEvent> connected;
		private Action<ENetEvent> received;
		private Action<ENetEvent> disconnect;

		public ENetPeer(ENetHost host, Native.ENetPeer* peer)
		{
			if (peer == null)
			{
				throw new InvalidOperationException("No native peer.");
			}
			this.peer = peer;
			this.host = host;
			this.host.Peers.Add(this);
		}

		~ENetPeer()
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
			}
			this.host.Peers.Remove((int)this.peer->data);
			this.peer = null;
		}

		// peer连接上了调用该回调方法
		public Action<ENetEvent> Connected
		{
			get
			{
				if (connected == null)
				{
					return e => { };
				}
				return connected;
			}
			set
			{
				connected = value;
			}
		}

		public Action<ENetEvent> Received
		{
			get
			{
				if (received == null)
				{
					return e => { };
				}
				return received;
			}
			set
			{
				received = value;
			}
		}

		public Action<ENetEvent> Disconnect
		{
			get
			{
				if (disconnect == null)
				{
					return e => { };
				}
				return disconnect;
			}
			set
			{
				disconnect = value;
			}
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			Native.enet_peer_throttle_configure(this.peer, interval, acceleration, deceleration);
		}

		public void Send(byte channelID, byte[] data)
		{
			this.Send(channelID, data, 0, data.Length);
		}

		public void Send(byte channelID, byte[] data, int offset, int length)
		{
			using (var packet = new ENetPacket(data, offset, length))
			{
				this.Send(channelID, packet);
			}
		}

		public void Send(byte channelID, ENetPacket packet)
		{
			Native.enet_peer_send(this.peer, channelID, packet.NativeData);
		}

		public Task<ENetPacket> ReceiveAsync()
		{
			var tcs = new TaskCompletionSource<ENetPacket>();
			this.Received = e => tcs.TrySetResult(e.Packet);
			return tcs.Task;
		}

		public Task<bool> DisconnectAsync(uint data)
		{
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = e => tcs.TrySetResult(true);
			Native.enet_peer_disconnect(this.peer, data);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data)
		{
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = e => tcs.TrySetResult(true);
			Native.enet_peer_disconnect_later(this.peer, data);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			Native.enet_peer_disconnect_now(this.peer, data);
		}

		public void Ping()
		{
			Native.enet_peer_ping(this.peer);
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