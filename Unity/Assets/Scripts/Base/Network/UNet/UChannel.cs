using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Model
{
	internal class UChannel: AChannel
	{
		private readonly USocket socket;

		private TaskCompletionSource<byte[]> recvTcs;

		/// <summary>
		/// connect
		/// </summary>
		public UChannel(USocket socket, string host, int port, UService service): base(service, ChannelType.Connect)
		{
			this.socket = socket;
			this.service = service;
			this.RemoteAddress = host + ":" + port;
			this.socket.ConnectAsync(host, (ushort)port);
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

		public override void Send(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			if (this.Id == 0)
			{
				throw new Exception("UChannel已经被Dispose, 不能发送消息");
			}
			this.socket.SendAsync(buffer, channelID, flags);
		}

		public override void Send(List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
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
			this.socket.SendAsync(buffer, channelID, flags);
		}

		public override Task<byte[]> Recv()
		{
			if (this.Id == 0)
			{
				throw new Exception("UChannel已经被Dispose, 不能接收消息");
			}
			
			var recvQueue = this.socket.RecvQueue;
			if (recvQueue.Count > 0)
			{
				return Task.FromResult(recvQueue.Dequeue());
			}

			recvTcs = new TaskCompletionSource<byte[]>();
			return recvTcs.Task;
		}

		private void OnRecv()
		{
			var tcs = this.recvTcs;
			this.recvTcs = null;
			tcs?.SetResult(this.socket.RecvQueue.Dequeue());
		}
	}
}