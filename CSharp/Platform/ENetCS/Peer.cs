using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ENet
{
	public class Peer: IDisposable
	{
		private readonly Host host;
		private readonly PeerEvent peerEvent = new PeerEvent();
		private IntPtr peer;

		public Peer(Host host, IntPtr peer)
		{
			this.host = host;
			this.peer = peer;
			host.PeersManager.Add(peer, this);
		}

		~Peer()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.peer == IntPtr.Zero)
			{
				return;
			}

			this.host.PeersManager.Remove(this.peer);
			NativeMethods.enet_peer_reset(this.peer);
			this.peer = IntPtr.Zero;
		}

		private ENetPeer Struct
		{
			get
			{
				if (this.peer == IntPtr.Zero)
				{
					return new ENetPeer();
				}
				return (ENetPeer) Marshal.PtrToStructure(this.peer, typeof (ENetPeer));
			}
			set
			{
				Marshal.StructureToPtr(value, this.peer, false);
			}
		}

		public PeerEvent PeerEvent
		{
			get
			{
				return this.peerEvent;
			}
		}

		public PeerState State
		{
			get
			{
				if (this.peer == IntPtr.Zero)
				{
					return PeerState.Uninitialized;
				}
				return this.Struct.state;
			}
		}

		public void Ping()
		{
			NativeMethods.enet_peer_ping(this.peer);
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			NativeMethods.enet_peer_throttle_configure(this.peer, interval, acceleration, deceleration);
		}

		public void Send(byte channelID, string data)
		{
			using (var packet = new Packet(data))
			{
				this.Send(channelID, packet);
			}
		}

		public void Send(byte channelID, Packet packet)
		{
			NativeMethods.enet_peer_send(this.peer, channelID, packet.NativePtr);
		}

		public Task<Packet> ReceiveAsync()
		{
			var tcs = new TaskCompletionSource<Packet>();
			this.PeerEvent.Received += e => tcs.TrySetResult(e.Packet);
			return tcs.Task;
		}

		public Task<bool> DisconnectAsync(uint data = 0)
		{
			var tcs = new TaskCompletionSource<bool>();
			this.PeerEvent.Disconnect += e => tcs.TrySetResult(true);
			NativeMethods.enet_peer_disconnect(this.peer, data);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			var tcs = new TaskCompletionSource<bool>();
			this.PeerEvent.Disconnect += e => tcs.TrySetResult(true);
			NativeMethods.enet_peer_disconnect_later(this.peer, data);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			NativeMethods.enet_peer_disconnect_now(this.peer, data);
		}
	}
}