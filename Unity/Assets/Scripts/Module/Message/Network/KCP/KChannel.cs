using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ETModel
{
	public struct WaitSendBuffer
	{
		public byte[] Bytes;
		public int Index;
		public int Length;

		public WaitSendBuffer(byte[] bytes, int index, int length)
		{
			this.Bytes = bytes;
			this.Index = index;
			this.Length = length;
		}
	}

	public class KChannel : AChannel
	{
		private Socket socket;

		private KCP kcp;

		private readonly Queue<WaitSendBuffer> sendBuffer = new Queue<WaitSendBuffer>();

		private bool isConnected;
		private readonly IPEndPoint remoteEndPoint;

		private uint lastRecvTime;

		private readonly byte[] cacheBytes = new byte[ushort.MaxValue];

		public uint Conn;

		public uint RemoteConn;

		public uint lastConnectTime;

		// accept
		public KChannel(uint conn, uint remoteConn, Socket socket, IPEndPoint remoteEndPoint, KService kService) : base(kService, ChannelType.Accept)
		{
			this.Id = conn;
			this.Conn = conn;
			this.RemoteConn = remoteConn;
			this.remoteEndPoint = remoteEndPoint;
			this.socket = socket;
			kcp = new KCP(this.RemoteConn, this);
			kcp.SetOutput(this.Output);
			kcp.NoDelay(1, 10, 2, 1);  //fast
			this.isConnected = true;

			this.lastRecvTime = kService.TimeNow;
		}

		// connect
		public KChannel(uint conn, Socket socket, IPEndPoint remoteEndPoint, KService kService) : base(kService, ChannelType.Connect)
		{
			this.Id = conn;
			this.Conn = conn;
			this.socket = socket;

			this.remoteEndPoint = remoteEndPoint;
			this.lastRecvTime = kService.TimeNow;
			this.Connect(kService.TimeNow);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			for (int i = 0; i < 4; i++)
			{
				this.DisConnect();
			}

			this.socket = null;
		}

		private KService GetService()
		{
			return (KService)this.service;
		}

		public void HandleConnnect(uint responseConn)
		{
			if (this.isConnected)
			{
				return;
			}
			this.isConnected = true;
			this.RemoteConn = responseConn;
			this.kcp = new KCP(responseConn, this);
			kcp.SetOutput(this.Output);
			kcp.NoDelay(1, 10, 2, 1);  //fast

			HandleSend();
		}

		public void HandleAccept(uint requestConn)
		{
			cacheBytes.WriteTo(0, KcpProtocalType.ACK);
			cacheBytes.WriteTo(4, requestConn);
			cacheBytes.WriteTo(8, this.Conn);
			this.socket.SendTo(cacheBytes, 0, 12, SocketFlags.None, remoteEndPoint);
		}

		/// <summary>
		/// 发送请求连接消息
		/// </summary>
		private void Connect(uint timeNow)
		{
			cacheBytes.WriteTo(0, KcpProtocalType.SYN);
			cacheBytes.WriteTo(4, this.Conn);
			this.socket.SendTo(cacheBytes, 0, 8, SocketFlags.None, remoteEndPoint);

			// 200毫秒后再次update发送connect请求
			this.GetService().AddToNextTimeUpdate(timeNow + 200, this.Id);
		}

		private void DisConnect()
		{
			cacheBytes.WriteTo(0, KcpProtocalType.FIN);
			cacheBytes.WriteTo(4, this.Conn);
			cacheBytes.WriteTo(8, this.RemoteConn);
			//Log.Debug($"client disconnect: {this.Conn}");
			this.socket.SendTo(cacheBytes, 0, 12, SocketFlags.None, remoteEndPoint);
		}

		public void Update(uint timeNow)
		{
			// 如果还没连接上，发送连接请求
			if (!this.isConnected)
			{
				Connect(timeNow);
				return;
			}
			
			// 超时断开连接
			if (timeNow - this.lastRecvTime > 20 * 1000)
			{
				this.OnError((int)SocketError.Disconnecting);
				return;
			}
			this.kcp.Update(timeNow);

			uint nextUpdateTime = this.kcp.Check(timeNow);
			this.GetService().AddToNextTimeUpdate(nextUpdateTime, this.Id);
		}

		private void HandleSend()
		{
			while (true)
			{
				if (this.sendBuffer.Count <= 0)
				{
					break;
				}
				WaitSendBuffer buffer = this.sendBuffer.Dequeue();
				this.KcpSend(buffer.Bytes, buffer.Index, buffer.Length);
			}
		}

		public void HandleRecv(byte[] date, int length, uint timeNow)
		{
			this.kcp.Input(date, 0, length);
			this.GetService().AddToUpdate(this.Id);

			while (true)
			{
				int n = kcp.PeekSize();
				if (n == 0)
				{
					this.OnError((int)SocketError.NetworkReset);
					return;
				}
				int count = this.kcp.Recv(this.cacheBytes, 0, this.cacheBytes.Length);
				if (count <= 0)
				{
					return;
				}

				lastRecvTime = timeNow;

				this.packet.Flag = this.cacheBytes[0];
				this.packet.Opcode = BitConverter.ToUInt16(this.cacheBytes, 1);
				this.packet.Bytes = this.cacheBytes;
				this.packet.Offset = Packet.Index;
				this.packet.Length = (ushort) (count - Packet.Index);
				this.OnRead(packet);
			}
		}
		
		public Packet packet = new Packet();

		public void Output(byte[] bytes, int count, object user)
		{
			this.socket.SendTo(bytes, 0, count, SocketFlags.None, this.remoteEndPoint);
		}

		private void KcpSend(byte[] buffers, int index, int length)
		{
			this.kcp.Send(buffers, index, length);
			this.GetService().AddToUpdate(this.Id);
		}

		public override void Start()
		{
		}

		public override void Send(byte[] buffer, int index, int length)
		{
			if (isConnected)
			{
				this.KcpSend(buffer, index, length);
				return;
			}
			this.sendBuffer.Enqueue(new WaitSendBuffer(buffer, index, length));
		}

		public override void Send(List<byte[]> buffers)
		{
			ushort size = (ushort)buffers.Select(b => b.Length).Sum();
			byte[] bytes;
			if (!this.isConnected)
			{
				bytes = this.cacheBytes;
			}
			else
			{
				bytes = new byte[size];
			}

			int index = 0;
			foreach (byte[] buffer in buffers)
			{
				Array.Copy(buffer, 0, bytes, index, buffer.Length);
				index += buffer.Length;
			}

			Send(bytes, 0, size);
		}
	}
}
