using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Network;

namespace UNet
{
	public sealed class USocket: IDisposable
	{
		private IntPtr peerPtr = IntPtr.Zero;
		private readonly UPoller service;
		private readonly LinkedList<byte[]> recvBuffer = new LinkedList<byte[]>();

		public Action<UEvent> Connected { get; private set; }
		public Action<UEvent> Received { get; private set; }
		public Action<UEvent> Disconnect { get; private set; }
		public Action<int> Error { get; set; }

		public USocket(UPoller service)
		{
			this.service = service;
		}

		private void Dispose(bool disposing)
		{
			if (this.peerPtr == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.EnetPeerReset(this.peerPtr);
			this.peerPtr = IntPtr.Zero;
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

		public Task<bool> ConnectAsync(string hostName, ushort port)
		{
			var tcs = new TaskCompletionSource<bool>();
			UAddress address = new UAddress { Host = hostName, Port = port };
			ENetAddress nativeAddress = address.Struct;
			this.peerPtr = NativeMethods.EnetHostConnect(
				this.service.HostPtr, ref nativeAddress, NativeMethods.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT, 0);
			if (this.peerPtr == IntPtr.Zero)
			{
				throw new UException("host connect call failed.");
			}
			this.service.USocketManager.Add(this.peerPtr, this);
			this.Connected = eEvent =>
			{
				if (eEvent.EventState == EventState.DISCONNECTED)
				{
					tcs.TrySetException(new UException("socket disconnected in connect"));
				}
				tcs.TrySetResult(true);
			};
			return tcs.Task;
		}

		public Task<bool> AcceptAsync()
		{
			if (this.service.USocketManager.ContainsKey(IntPtr.Zero))
			{
				throw new UException("do not accept twice!");
			}

			var tcs = new TaskCompletionSource<bool>();

			// 如果有请求连接缓存的包,从缓存中取
			if (this.service.ConnEEvents.Count > 0)
			{
				UEvent uEvent = this.service.ConnEEvents.First.Value;
				this.service.ConnEEvents.RemoveFirst();

				this.PeerPtr = uEvent.PeerPtr;
				this.service.USocketManager.Add(this.PeerPtr, this);
				tcs.TrySetResult(true);
			}
			else
			{
				this.service.USocketManager.Add(this.PeerPtr, this);
				this.Connected = eEvent =>
				{
					if (eEvent.EventState == EventState.DISCONNECTED)
					{
						tcs.TrySetException(new UException("socket disconnected in accpet"));
					}

					this.service.USocketManager.Remove(IntPtr.Zero);

					this.PeerPtr = eEvent.PeerPtr;
					this.service.USocketManager.Add(this.PeerPtr, this);
					tcs.TrySetResult(true);
				};
			}
			return tcs.Task;
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
					if (eEvent.EventState == EventState.DISCONNECTED)
					{
						tcs.TrySetException(new UException("socket disconnected in receive"));
					}

					using (UPacket packet = new UPacket(eEvent.PacketPtr))
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
			this.PeerPtr = IntPtr.Zero;
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = eEvent => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public Task<bool> DisconnectLaterAsync(uint data = 0)
		{
			NativeMethods.EnetPeerDisconnectLater(this.peerPtr, data);
			// EnetPeerDisconnect会reset Peer,这里设置为0,防止再次Dispose
			this.PeerPtr = IntPtr.Zero;
			var tcs = new TaskCompletionSource<bool>();
			this.Disconnect = eEvent => tcs.TrySetResult(true);
			return tcs.Task;
		}

		public void DisconnectNow(uint data)
		{
			NativeMethods.EnetPeerDisconnectNow(this.peerPtr, data);
			// EnetPeerDisconnect会reset Peer,这里设置为0,防止再次Dispose
			this.PeerPtr = IntPtr.Zero;
		}

		internal void OnConnected(UEvent uEvent)
		{
			if (this.Connected == null)
			{
				return;
			}
			Action<UEvent> localConnected = this.Connected;
			this.Connected = null;
			// 此调用将让await ConnectAsync返回,所以null必须在此之前设置
			localConnected(uEvent);
		}

		internal void OnReceived(UEvent uEvent)
		{
			// 如果应用层还未调用readasync则将包放到缓存队列
			if (this.Received == null)
			{
				using (UPacket packet = new UPacket(uEvent.PacketPtr))
				{
					byte[] bytes = packet.Bytes;
					this.recvBuffer.AddLast(bytes);
				}
			}
			else
			{
				Action<UEvent> localReceived = this.Received;
				this.Received = null;
				// 此调用将让await ReadAsync返回,所以null必须在此之前设置
				localReceived(uEvent);
			}
		}

		internal void OnDisconnect(UEvent uEvent)
		{
			if (this.Disconnect == null)
			{
				return;
			}
			this.Disconnect(uEvent);
		}

		internal void OnError(int errorCode)
		{
			if (this.Error == null)
			{
				return;
			}
			this.Error(errorCode);
		}
	}
}