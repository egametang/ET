using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Network;

namespace UNet
{
	internal sealed class USocket : IDisposable
	{
		private IntPtr peerPtr;
		private readonly LinkedList<byte[]> recvBuffer = new LinkedList<byte[]>();

		public Action<ENetEvent> Connected { get; set; }
		public Action<ENetEvent> Received { get; private set; }
		public Action<ENetEvent> Disconnect { get; private set; }
		public Action<int> Error { get; set; }

		private void Dispose(bool disposing)
		{
			if (this.peerPtr == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.EnetPeerReset(this.peerPtr);
			this.peerPtr = IntPtr.Zero;
		}

		public USocket(IntPtr peerPtr)
		{
			this.peerPtr = peerPtr;
		}

		~USocket()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public IntPtr PeerPtr
		{
			get
			{
				return this.peerPtr;
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
				ENetPeer peer = (ENetPeer) Marshal.PtrToStructure(this.peerPtr, typeof (ENetPeer));
				return peer;
			}
			set
			{
				Marshal.StructureToPtr(value, this.peerPtr, false);
			}
		}

		public string RemoteAddress
		{
			get
			{
				ENetPeer peer = this.Struct;
				return peer.Address.Host + ":" + peer.Address.Port;
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

		public void SendAsync(byte[] data, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			UPacket packet = new UPacket(data, flags);
			NativeMethods.EnetPeerSend(this.peerPtr, channelID, packet.PacketPtr);
			// enet_peer_send函数会自动删除packet,设置为0,防止Dispose或者析构函数再次删除
			packet.PacketPtr = IntPtr.Zero;
		}

		public Task<byte[]> RecvAsync()
		{
			var tcs = new TaskCompletionSource<byte[]>();

			// 如果有缓存的包,从缓存中取
			if (this.recvBuffer.Count > 0)
			{
				byte[] bytes = this.recvBuffer.First.Value;
				this.recvBuffer.RemoveFirst();
				tcs.TrySetResult(bytes);
			}
			// 没有缓存封包,设置回调等待
			else
			{
				this.Received = eEvent =>
				{
					if (eEvent.Type == EventType.Disconnect)
					{
						tcs.TrySetException(new UException("socket disconnected in receive"));
					}

					using (UPacket packet = new UPacket(eEvent.Packet))
					{
						byte[] bytes = packet.Bytes;
						tcs.TrySetResult(bytes);
					}
				};
			}
			return tcs.Task;
		}

		public Task<bool> DisconnectAsync(uint data = 0)
		{
			NativeMethods.EnetPeerDisconnect(this.peerPtr, data);
			// EnetPeerDisconnect会reset Peer,这里设置为0,防止再次Dispose
			this.peerPtr = IntPtr.Zero;
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = eEvent => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			NativeMethods.EnetPeerDisconnectLater(this.peerPtr, data);
			// EnetPeerDisconnect会reset Peer,这里设置为0,防止再次Dispose
			this.peerPtr = IntPtr.Zero;
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = eEvent => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			NativeMethods.EnetPeerDisconnectNow(this.peerPtr, data);
			// EnetPeerDisconnect会reset Peer,这里设置为0,防止再次Dispose
			this.peerPtr = IntPtr.Zero;
		}

		internal void OnConnected(ENetEvent eNetEvent)
		{
			if (this.Connected == null)
			{
				return;
			}
			Action<ENetEvent> localConnected = this.Connected;
			this.Connected = null;
			// 此调用将让await ConnectAsync返回,所以null必须在此之前设置
			localConnected(eNetEvent);
		}

		internal void OnReceived(ENetEvent eNetEvent)
		{
			// 如果应用层还未调用readasync则将包放到缓存队列
			if (this.Received == null)
			{
				using (UPacket packet = new UPacket(eNetEvent.Packet))
				{
					byte[] bytes = packet.Bytes;
					this.recvBuffer.AddLast(bytes);
				}
			}
			else
			{
				Action<ENetEvent> localReceived = this.Received;
				this.Received = null;
				// 此调用将让await ReadAsync返回,所以null必须在此之前设置
				localReceived(eNetEvent);
			}
		}

		internal void OnDisconnect(ENetEvent eNetEvent)
		{
			if (this.Disconnect == null)
			{
				return;
			}

			Action<ENetEvent> localDisconnect = this.Disconnect;
			this.Disconnect = null;
			localDisconnect(eNetEvent);
		}
	}
}