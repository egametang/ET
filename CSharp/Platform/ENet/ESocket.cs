using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Log;

namespace ENet
{
	public sealed class ESocket: IDisposable
	{
		private IntPtr peerPtr = IntPtr.Zero;
		private readonly EService service;
		private readonly LinkedList<EEvent> recvEEvents = new LinkedList<EEvent>();

		public Action<EEvent> Connected { get; set; }
		public Action<EEvent> Received { get; set; }
		public Action<EEvent> Disconnect { get; set; }

		public ESocket(EService service)
		{
			this.service = service;
		}

		public void Dispose()
		{
			if (this.peerPtr == IntPtr.Zero)
			{
				return;
			}
			NativeMethods.EnetPeerReset(this.peerPtr);
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
			NativeMethods.EnetPeerPing(this.peerPtr);
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			NativeMethods.EnetPeerThrottleConfigure(this.peerPtr, interval, acceleration, deceleration);
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
			this.peerPtr = NativeMethods.EnetHostConnect(
				this.service.HostPtr, ref nativeAddress, channelLimit, data);
			if (this.peerPtr == IntPtr.Zero)
			{
				throw new EException("host connect call failed.");
			}
			this.service.PeersManager.Add(this.peerPtr, this);
			this.Connected = e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> AcceptAsync()
		{
			if (this.service.PeersManager.ContainsKey(IntPtr.Zero))
			{
				throw new EException("do not accept twice!");
			}

			var tcs = new TaskCompletionSource<bool>();

			// 如果有请求连接缓存的包,从缓存中取
			if (this.service.ConnEEvents.Count > 0)
			{
				EEvent eEvent = this.service.ConnEEvents.First.Value;
				this.service.ConnEEvents.RemoveFirst();

				this.PeerPtr = eEvent.PeerPtr;
				this.service.PeersManager.Add(this.PeerPtr, this);

				tcs.TrySetResult(true);
			}
			else
			{
				this.service.PeersManager.Add(this.PeerPtr, this);
				this.Connected = eEvent =>
				{
					this.service.PeersManager.Remove(IntPtr.Zero);

					this.PeerPtr = eEvent.PeerPtr;
					this.service.PeersManager.Add(this.PeerPtr, this);
					tcs.TrySetResult(true); 
				};
			}
			return tcs.Task;
		}

		public void WriteAsync(byte[] data, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			var packet = new EPacket(data, flags);
			NativeMethods.EnetPeerSend(this.peerPtr, channelID, packet.PacketPtr);
			// enet_peer_send函数会自动删除packet,设置为0,防止Dispose或者析构函数再次删除
			packet.PacketPtr = IntPtr.Zero;
		}

		public Task<byte[]> ReadAsync()
		{
			var tcs = new TaskCompletionSource<byte[]>();

			// 如果有缓存的包,从缓存中取
			if (this.recvEEvents.Count > 0)
			{
				EEvent eEvent = this.recvEEvents.First.Value;
				this.recvEEvents.RemoveFirst();
				using (var packet = new EPacket(eEvent.PacketPtr))
				{
					var bytes = packet.Bytes;
					tcs.TrySetResult(bytes);
				}
			}
			// 没有缓存封包,设置回调等待
			else
			{
				this.Received = eEvent => 
				{
					if (eEvent.EventState == EventState.DISCONNECTED)
					{
						tcs.TrySetException(new EException("Peer Disconnected In Received"));
					}

					using (var packet = new EPacket(eEvent.PacketPtr))
					{
						var bytes = packet.Bytes;
						tcs.TrySetResult(bytes);
					}
				};
			}
			return tcs.Task;
		}

		public Task<bool> DisconnectAsync(uint data = 0)
		{
			NativeMethods.EnetPeerDisconnect(this.peerPtr, data);
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			NativeMethods.EnetPeerDisconnectLater(this.peerPtr, data);
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = e => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			NativeMethods.EnetPeerDisconnectNow(this.peerPtr, data);
		}

		internal void OnConnected(EEvent eEvent)
		{
			if (this.Connected == null)
			{
				return;
			}
			Action<EEvent> localConnected = this.Connected;
			this.Connected = null;
			// 此调用将让await ConnectAsync返回,所以null必须在此之前设置
			localConnected(eEvent);
		}

		internal void OnReceived(EEvent eEvent)
		{
			if (this.Received == null)
			{
				this.recvEEvents.AddLast(eEvent);
			}
			else
			{
				Action<EEvent> localReceived = this.Received;
				this.Received = null;
				// 此调用将让await ReadAsync返回,所以null必须在此之前设置
				localReceived(eEvent);	
			}
		}

		internal void OnDisconnect(EEvent e)
		{
			if (this.Disconnect == null)
			{
				return;
			}
			this.Disconnect(e);
		}
	}
}