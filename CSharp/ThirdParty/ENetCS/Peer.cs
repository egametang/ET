using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ENet
{
	public class Peer : IDisposable
	{
		private static readonly PeerEventsManager peerEventsManager = new PeerEventsManager();

		public static PeerEventsManager PeerEventsManager
		{
			get
			{
				return peerEventsManager;
			}
		}

		private IntPtr peer;

		public Peer(IntPtr peer)
		{
			this.peer = peer;
			PeerEventsManager.Add(peer);
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
			if (this.peer == IntPtr.Zero)
			{
				return;
			}

			PeerEventsManager.Remove(this.peer);
			Native.enet_peer_reset(this.peer);
			this.peer = IntPtr.Zero;
		}

		public ENetPeer Struct
		{
			get
			{
				if (this.peer == IntPtr.Zero)
				{
					return new ENetPeer();
				}
				return (ENetPeer)Marshal.PtrToStructure(this.peer, typeof(ENetPeer));
			}
			set
			{
				Marshal.StructureToPtr(value, this.peer, false);
			}
		}

		public IntPtr NativePtr
		{
			get
			{
				return this.peer;
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
				return Struct.state;
			}
		}

		public void Ping()
		{
			Native.enet_peer_ping(this.peer);
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			Native.enet_peer_throttle_configure(this.peer, interval, acceleration, deceleration);
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
			Native.enet_peer_send(this.peer, channelID, packet.NativePtr);
		}

		public Task<Packet> ReceiveAsync()
		{
			var tcs = new TaskCompletionSource<Packet>();
			PeerEventsManager[this.peer].Received += e => tcs.TrySetResult(e.Packet);
			return tcs.Task;
		}

		public Task<bool> DisconnectAsync(uint data = 0)
		{
			var tcs = new TaskCompletionSource<bool>();
			PeerEventsManager[this.peer].Disconnect += e => tcs.TrySetResult(true);
			Native.enet_peer_disconnect(this.peer, data);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			var tcs = new TaskCompletionSource<bool>();
			PeerEventsManager[this.peer].Disconnect += e => tcs.TrySetResult(true);
			Native.enet_peer_disconnect_later(this.peer, data);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			Native.enet_peer_disconnect_now(this.peer, data);
		}
	}
}