using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Model
{
	internal class UChannel: AChannel
	{
		private readonly USocket socket;

		private TaskCompletionSource<Packet> recvTcs;
		
		/// <summary>
		/// connect
		/// </summary>
		public UChannel(USocket socket, IPEndPoint ipEndPoint, UService service): base(service, ChannelType.Connect)
		{
			this.socket = socket;
			this.service = service;
			this.RemoteAddress = ipEndPoint;
			this.socket.ConnectAsync(ipEndPoint);
			this.socket.Received += this.OnRecv;
			this.socket.Disconnect += () => { this.OnError(this, SocketError.SocketError); };
		}

		/// <summary>
		/// accept
		/// </summary>
		public UChannel(USocket socket, UService service) : base(service, ChannelType.Accept)
		{
			this.socket = socket;
			this.service = service;
			this.RemoteAddress = socket.RemoteAddress;
			this.socket.Received += this.OnRecv;
			this.socket.Disconnect += () => { this.OnError(this, SocketError.SocketError); };
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();
			this.socket.Dispose();
		}

		public override void Send(byte[] buffer)
		{
			if (this.Id == 0)
			{
				throw new Exception("UChannel已经被Dispose, 不能发送消息");
			}
			this.socket.SendAsync(buffer);
		}

		public override void Send(List<byte[]> buffers)
		{
			if (this.Id == 0)
			{
				throw new Exception("UChannel已经被Dispose, 不能发送消息");
			}
			int size = buffers.Select(b => b.Length).Sum();
			var buffer = new byte[size];
			int index = 0;
			foreach (byte[] bytes in buffers)
			{
				Array.Copy(bytes, 0, buffer, index, bytes.Length);
				index += bytes.Length;
			}
			this.socket.SendAsync(buffer);
		}

		public override Task<Packet> Recv()
		{
			if (this.Id == 0)
			{
				throw new Exception("UChannel已经被Dispose, 不能接收消息");
			}
			
			var recvQueue = this.socket.RecvQueue;
			if (recvQueue.Count > 0)
			{
				byte[] recvByte = recvQueue.Dequeue();
				return Task.FromResult(new Packet(recvByte));
			}

			recvTcs = new TaskCompletionSource<Packet>();
			return recvTcs.Task;
		}

		private void OnRecv()
		{
			var tcs = this.recvTcs;
			this.recvTcs = null;
			byte[] recvByte = this.socket.RecvQueue.Dequeue();
			tcs?.SetResult(new Packet(recvByte));
		}
	}
}