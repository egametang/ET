using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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

	public class KChannel: AChannel
	{
		private UdpClient socket;

		private Kcp kcp;

		private readonly CircularBuffer recvBuffer = new CircularBuffer();
		private readonly Queue<WaitSendBuffer> sendBuffer = new Queue<WaitSendBuffer>();

		private readonly PacketParser parser;
		private bool isConnected;
		private readonly IPEndPoint remoteEndPoint;

		private TaskCompletionSource<Packet> recvTcs;

		private uint lastRecvTime;

		private readonly byte[] cacheBytes = new byte[ushort.MaxValue];

		public uint Conn;

		public uint RemoteConn;

		// accept
		public KChannel(uint conn, uint remoteConn, UdpClient socket, IPEndPoint remoteEndPoint, KService kService): base(kService, ChannelType.Accept)
		{
			this.Id = conn;
			this.Conn = conn;
			this.RemoteConn = remoteConn;
			this.remoteEndPoint = remoteEndPoint;
			this.socket = socket;
			kcp = new Kcp(this.RemoteConn, this.Output);
			kcp.SetMtu(512);
			kcp.NoDelay(1, 10, 2, 1);  //fast
			this.isConnected = true;
			this.lastRecvTime = kService.TimeNow;
		}

		// connect
		public KChannel(uint conn, UdpClient socket, IPEndPoint remoteEndPoint, KService kService): base(kService, ChannelType.Connect)
		{
			this.Id = conn;
			this.Conn = conn;
			this.socket = socket;
			this.parser = new PacketParser(this.recvBuffer);

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
			this.kcp = new Kcp(responseConn, this.Output);
			kcp.SetMtu(512);
			kcp.NoDelay(1, 10, 2, 1);  //fast

			HandleSend();
		}

		public void HandleAccept(uint requestConn)
		{
			cacheBytes.WriteTo(0, KcpProtocalType.ACK);
			cacheBytes.WriteTo(4, requestConn);
			cacheBytes.WriteTo(8, this.Conn);
			this.socket.Send(cacheBytes, 12, remoteEndPoint);
		}

		/// <summary>
		/// 发送请求连接消息
		/// </summary>
		private void Connect(uint timeNow)
		{
			cacheBytes.WriteTo(0, KcpProtocalType.SYN);
			cacheBytes.WriteTo(4, this.Conn);
			//Log.Debug($"client connect: {this.Conn}");
			this.socket.Send(cacheBytes, 8, remoteEndPoint);

			// 200毫秒后再次update发送connect请求
			this.GetService().AddToNextTimeUpdate(timeNow + 200, this.Id);
		}

		private void DisConnect()
		{
			cacheBytes.WriteTo(0, KcpProtocalType.FIN);
			cacheBytes.WriteTo(4, this.Conn);
			cacheBytes.WriteTo(8, this.RemoteConn);
			//Log.Debug($"client disconnect: {this.Conn}");
			this.socket.Send(cacheBytes, 12, remoteEndPoint);
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
				this.OnError(SocketError.Disconnecting);
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

		public void HandleRecv(byte[] date, uint timeNow)
		{
			this.kcp.Input(date);
			// 加入update队列
			this.GetService().AddToUpdate(this.Id);

			while (true)
			{
				int n = kcp.PeekSize();
				if (n == 0)
				{
					this.OnError(SocketError.NetworkReset);
					return;
				}
				int count = this.kcp.Recv(this.cacheBytes);
				if (count <= 0)
				{
					return;
				}

				lastRecvTime = timeNow;

				// 收到的数据放入缓冲区
				byte[] sizeBuffer = BitConverter.GetBytes((ushort)count);
				this.recvBuffer.Write(sizeBuffer, 0, sizeBuffer.Length);
				this.recvBuffer.Write(cacheBytes, 0, count);

				if (this.recvTcs != null)
				{
					bool isOK = this.parser.Parse();
					if (isOK)
					{
						Packet pkt = this.parser.GetPacket();
						var tcs = this.recvTcs;
						this.recvTcs = null;
						tcs.SetResult(pkt);
					}
				}
			}
		}

		public void Output(byte[] bytes, int count)
		{
			this.socket.Send(bytes, count, this.remoteEndPoint);
		}

		private void KcpSend(byte[] buffers, int index, int length)
		{
			this.kcp.Send(buffers, index, length);
			this.GetService().AddToUpdate(this.Id);
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

		public override Task<Packet> Recv()
		{
			if (this.IsDisposed)
			{
				throw new Exception("KChannel已经被Dispose, 不能接收消息");
			}

			bool isOK = this.parser.Parse();
			if (isOK)
			{
				Packet packet = this.parser.GetPacket();
				return Task.FromResult(packet);
			}

			recvTcs = new TaskCompletionSource<Packet>();
			return recvTcs.Task;
		}
	}
}
