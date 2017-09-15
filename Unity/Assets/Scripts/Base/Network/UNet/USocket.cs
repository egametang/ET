using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace Model
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
		private readonly EQueue<byte[]> recvQueue = new EQueue<byte[]>();
		private readonly EQueue<BufferInfo> sendQueue = new EQueue<BufferInfo>();
		private bool isConnected;
		private Action disconnect;
		private Action received;

		public event Action Received
		{
			add
			{
				this.received += value;
			}
			remove
			{
				this.received -= value;
			}
		}

		public event Action Disconnect
		{
			add
			{
				this.disconnect += value;
			}
			remove
			{
				this.disconnect -= value;
			}
		}

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
			NativeMethods.enet_peer_disconnect_now(this.PeerPtr, 0);
			this.PeerPtr = IntPtr.Zero;
		}

		public string RemoteAddress
		{
			get
			{
				ENetPeer peer = this.Struct;
				IPAddress ipaddr = new IPAddress(peer.Address.Host);
				return $"{ipaddr}:{peer.Address.Port}";
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
		
		public EQueue<byte[]> RecvQueue
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

			this.PeerPtr = NativeMethods.enet_host_connect(this.poller.Host, ref nativeAddress, 2, 0);
			if (this.PeerPtr == IntPtr.Zero)
			{
				throw new Exception($"host connect call failed, {host}:{port}");
			}
			this.poller.USocketManager.Add(this.PeerPtr, this);
		}

		public void SendAsync(byte[] data, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			if (this.PeerPtr == IntPtr.Zero)
			{
				throw new Exception($"USocket 已经被Dispose,不能发送数据!");
			}
			if (!isConnected)
			{
				sendQueue.Enqueue(new BufferInfo { Buffer = data, ChannelID = channelID, Flags = flags });
				return;
			}
			UPacket packet = new UPacket(data, flags);
			NativeMethods.enet_peer_send(this.PeerPtr, channelID, packet.PacketPtr);
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
			this.received();
		}

		internal void OnDisconnect(ENetEvent eNetEvent)
		{
			disconnect();
		}
	}
}