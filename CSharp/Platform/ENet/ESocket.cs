using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ENet
{
	public sealed class ESocket: IDisposable
	{
		private readonly ESocketEvent eSocketEvent = new ESocketEvent();
		private IntPtr peerPtr = IntPtr.Zero;
		private readonly IOService service;

		public ESocket(IOService service)
		{
			this.service = service;
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

		public ESocketEvent ESocketEvent
		{
			get
			{
				return this.eSocketEvent;
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
				return this.Struct.State;
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

		public Task<bool> ConnectAsync(
			string hostName, ushort port,
			uint channelLimit = NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT,
			uint data = 0)
		{
			if (channelLimit > NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
			{
				throw new ArgumentOutOfRangeException("channelLimit");
			}

			var tcs = new TaskCompletionSource<bool>();
			var address = new Address { HostName = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			this.peerPtr = NativeMethods.enet_host_connect(
				this.service.HostPtr, ref nativeAddress, channelLimit, data);
			if (this.peerPtr == IntPtr.Zero)
			{
				throw new ENetException("Host connect call failed.");
			}
			this.service.PeersManager.Add(this.peerPtr, this);
			this.ESocketEvent.Connected += e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> AcceptAsync()
		{
			if (this.service.PeersManager.ContainsKey(IntPtr.Zero))
			{
				throw new ENetException("Do Not Accept Twice!");
			}
			var tcs = new TaskCompletionSource<bool>();
			this.service.PeersManager.Add(this.PeerPtr, this);
			this.ESocketEvent.Connected += e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public void WriteAsync(byte[] data, byte channelID = 0, PacketFlags flags = PacketFlags.None)
		{
			var packet = new Packet(data, flags);
			NativeMethods.enet_peer_send(this.peerPtr, channelID, packet.PacketPtr);
			// enet_peer_send函数会自动删除packet,设置为0,防止Dispose或者析构函数再次删除
			packet.PacketPtr = IntPtr.Zero;
		}

		public Task<byte[]> ReadAsync()
		{
			var tcs = new TaskCompletionSource<byte[]>();
			this.ESocketEvent.Received += e =>
			{
				if (e.EventState == EventState.DISCONNECTED)
				{
					tcs.TrySetException(new ENetException("Peer Disconnected In Received"));
				}

				using (var packet = new Packet(e.PacketPtr))
				{
					var bytes = packet.Bytes;
					packet.Dispose();
					tcs.TrySetResult(bytes);
				}
			};
			return tcs.Task;
		}

		public Task<bool> DisconnectAsync(uint data = 0)
		{
			NativeMethods.enet_peer_disconnect(this.peerPtr, data);
			var tcs = new TaskCompletionSource<bool>();
			this.ESocketEvent.Disconnect += e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			NativeMethods.enet_peer_disconnect_later(this.peerPtr, data);
			var tcs = new TaskCompletionSource<bool>();
			this.ESocketEvent.Disconnect += e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			NativeMethods.enet_peer_disconnect_now(this.peerPtr, data);
		}
	}
}