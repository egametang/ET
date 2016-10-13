using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Base
{
	internal class BufferInfo
	{
		public byte[] Buffer { get; set; }
		public byte ChannelID { get; set; }
		public PacketFlags Flags { get; set; }
	}

	internal sealed class USocket: IDisposable
	{
		private readonly UPoller poller;
		public IntPtr PeerPtr { get; set; }
		private readonly Queue<byte[]> recvQueue = new Queue<byte[]>();
		private readonly Queue<BufferInfo> sendQueue = new Queue<BufferInfo>();
		private bool isConnected;
		public Action Disconnect;
		public Action Received;

		public USocket(IntPtr peerPtr, UPoller poller)
		{
			this.poller = poller;
			this.PeerPtr = peerPtr;
		}

		public USocket(UPoller poller)
		{
			this.poller = poller;
		}

		public void Dispose()
		{
			if (this.PeerPtr == IntPtr.Zero)
			{
				return;
			}

			poller.USocketManager.Remove(this.PeerPtr);
			NativeMethods.ENetPeerDisconnectNow(this.PeerPtr, 0);
			this.PeerPtr = IntPtr.Zero;
		}

		public string RemoteAddress
		{
			get
			{
				ENetPeer peer = this.Struct;
				return peer.Address.Host + ":" + peer.Address.Port;
			}
		}

		private ENetPeer Struct
		{
			get
			{
				if (this.PeerPtr == IntPtr.Zero)
				{
					return new ENetPeer();
				}
				ENetPeer peer = (ENetPeer)Marshal.PtrToStructure(this.PeerPtr, typeof(ENetPeer));
				return peer;
			}
			set
			{
				Marshal.StructureToPtr(value, this.PeerPtr, false);
			}
		}
		
		public Queue<byte[]> RecvQueue
		{
			get
			{
				return recvQueue;
			}
		}

		public void ConnectAsync(string host, ushort port)
		{
			UAddress address = new UAddress(host, port);
			ENetAddress nativeAddress = address.Struct;

			this.PeerPtr = NativeMethods.ENetHostConnect(this.poller.Host, ref nativeAddress, 2, 0);
			if (this.PeerPtr == IntPtr.Zero)
			{
				throw new Exception($"host connect call failed, {host}:{port}");
			}
			this.poller.USocketManager.Add(this.PeerPtr, this);
		}

		public void SendAsync(byte[] data, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			if (!isConnected)
			{
				sendQueue.Enqueue(new BufferInfo { Buffer = data, ChannelID = channelID, Flags = flags });
				return;
			}
			UPacket packet = new UPacket(data, flags);
			NativeMethods.ENetPeerSend(this.PeerPtr, channelID, packet.PacketPtr);
			// enet_peer_send函数会自动删除packet,设置为0,防止Dispose或者析构函数再次删除
			packet.PacketPtr = IntPtr.Zero;
		}

		internal void OnConnected()
		{
			isConnected = true;
			while (this.sendQueue.Count > 0)
			{
				BufferInfo info = this.sendQueue.Dequeue();
				this.SendAsync(info.Buffer, info.ChannelID, info.Flags);
			}
		}

		internal void OnAccepted()
		{
			isConnected = true;
		}

		internal void OnReceived(ENetEvent eNetEvent)
		{
			// 将包放到缓存队列
			using (UPacket packet = new UPacket(eNetEvent.Packet))
			{
				byte[] bytes = packet.Bytes;
				this.RecvQueue.Enqueue(bytes);
			}
		}

		internal void OnDisconnect(ENetEvent eNetEvent)
		{
			Disconnect();
		}
	}
}