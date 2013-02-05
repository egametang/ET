using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ENet
{
	public sealed class Peer: IDisposable
	{
		private readonly PeerEvent peerEvent = new PeerEvent();
		private IntPtr peerPtr;

		public Peer(IntPtr peerPtr)
		{
			this.peerPtr = peerPtr;
		}

		public void Dispose()
		{
			if (this.peerPtr == IntPtr.Zero)
			{
				return;
			}
			NativeMethods.enet_peer_reset(this.peerPtr);
			this.peerPtr = IntPtr.Zero;
		}

		public IntPtr PeerPtr
		{
			get
			{
				return this.peerPtr;
			}
			set
			{
				this.peerPtr = value;
			}
		}

		private ENetPeer Struct
		{
			get
			{
				if (this.peerPtr == IntPtr.Zero)
				{
					return new ENetPeer();
				}
				return (ENetPeer) Marshal.PtrToStructure(this.peerPtr, typeof (ENetPeer));
			}
			set
			{
				Marshal.StructureToPtr(value, this.peerPtr, false);
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
				if (this.peerPtr == IntPtr.Zero)
				{
					return PeerState.Uninitialized;
				}
				return this.Struct.state;
			}
		}

		public void Ping()
		{
			NativeMethods.enet_peer_ping(this.peerPtr);
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			NativeMethods.enet_peer_throttle_configure(this.peerPtr, interval, acceleration, deceleration);
		}

		public void WriteAsync(byte channelID, byte[] data)
		{
			var packet = new Packet(data);
			this.WriteAsync(channelID, packet);
		}

		public void WriteAsync(byte channelID, Packet packet)
		{
			NativeMethods.enet_peer_send(this.peerPtr, channelID, packet.PacketPtr);
			// enet_peer_send函数会自动删除packet,设置为0,防止Dispose或者析构函数再次删除
			packet.PacketPtr = IntPtr.Zero;
		}

		public Task<Packet> ReadAsync()
		{
			var tcs = new TaskCompletionSource<Packet>();
			this.PeerEvent.Received += e =>
			{
				if (e.EventState == EventState.DISCONNECTED)
				{
					tcs.TrySetException(new ENetException(3, "Peer Disconnected In Received"));
				}
				var packet = new Packet(e.PacketPtr);
				tcs.TrySetResult(packet);
			};
			return tcs.Task;
		}

		public Task<bool> DisconnectAsync(uint data = 0)
		{
			NativeMethods.enet_peer_disconnect(this.peerPtr, data);
			var tcs = new TaskCompletionSource<bool>();
			this.PeerEvent.Disconnect += e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			NativeMethods.enet_peer_disconnect_later(this.peerPtr, data);
			var tcs = new TaskCompletionSource<bool>();
			this.PeerEvent.Disconnect += e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			NativeMethods.enet_peer_disconnect_now(this.peerPtr, data);
		}
	}
}