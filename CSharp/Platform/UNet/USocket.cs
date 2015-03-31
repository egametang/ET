using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common.Network;

namespace UNet
{
	internal sealed class USocket: IDisposable
	{
		private readonly UPoller poller;
		private IntPtr peerPtr;
		private readonly Queue<byte[]> recvQueue = new Queue<byte[]>();

		public Action<ENetEvent> Connected { get; set; }
		public Action<ENetEvent> Received { get; private set; }
		public Action<ENetEvent> Disconnect { get; private set; }

		private void Dispose(bool disposing)
		{
			if (this.peerPtr == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.ENetPeerReset(this.peerPtr);
			this.peerPtr = IntPtr.Zero;
		}

		public USocket(IntPtr peerPtr, UPoller poller)
		{
			this.poller = poller;
			this.peerPtr = peerPtr;
		}

		public USocket(UPoller poller)
		{
			this.poller = poller;
		}

		~USocket()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
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
			NativeMethods.ENetPeerPing(this.peerPtr);
		}

		public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
		{
			NativeMethods.ENetPeerThrottleConfigure(this.peerPtr, interval, acceleration, deceleration);
		}

		public Task<bool> ConnectAsync(string hostName, ushort port)
		{
			var tcs = new TaskCompletionSource<bool>();
			UAddress address = new UAddress(hostName, port);
			ENetAddress nativeAddress = address.Struct;

			this.peerPtr = NativeMethods.ENetHostConnect(this.poller.Host, ref nativeAddress,
					NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT, 0);
			if (this.PeerPtr == IntPtr.Zero)
			{
				throw new UException("host connect call failed.");
			}
			this.poller.USocketManager.Add(this.PeerPtr, this);
			this.Connected = eEvent =>
			{
				if (eEvent.Type == EventType.Disconnect)
				{
					tcs.TrySetException(new UException("socket disconnected in connect"));
				}
				tcs.TrySetResult(true);
			};
			return tcs.Task;
		}

		public void SendAsync(byte[] data, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			UPacket packet = new UPacket(data, flags);
			NativeMethods.ENetPeerSend(this.peerPtr, channelID, packet.PacketPtr);
			// enet_peer_send函数会自动删除packet,设置为0,防止Dispose或者析构函数再次删除
			packet.PacketPtr = IntPtr.Zero;
		}

		public Task<byte[]> RecvAsync()
		{
			var tcs = new TaskCompletionSource<byte[]>();

			// 如果有缓存的包,从缓存中取
			if (this.recvQueue.Count > 0)
			{
				byte[] bytes = this.recvQueue.Dequeue();
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
			NativeMethods.ENetPeerDisconnect(this.peerPtr, data);
			// EnetPeerDisconnect会reset Peer,这里设置为0,防止再次Dispose
			this.peerPtr = IntPtr.Zero;
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = eEvent => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			NativeMethods.ENetPeerDisconnectLater(this.peerPtr, data);
			// EnetPeerDisconnect会reset Peer,这里设置为0,防止再次Dispose
			this.peerPtr = IntPtr.Zero;
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = eEvent => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			NativeMethods.ENetPeerDisconnectNow(this.peerPtr, data);
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
					this.recvQueue.Enqueue(bytes);
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