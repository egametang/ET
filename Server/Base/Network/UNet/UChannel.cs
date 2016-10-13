using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base
{
	internal class UChannel: AChannel
	{
		private readonly USocket socket;
		public string RemoteAddress { get; private set; }

		private TaskCompletionSource<byte[]> recvTcs;

		/// <summary>
		/// connect
		/// </summary>
		public UChannel(USocket socket, string host, int port, UService service): base(service)
		{
			this.socket = socket;
			this.service = service;
			this.RemoteAddress = host + ":" + port;
			this.socket.ConnectAsync(host, (ushort)port);
			this.socket.Received += this.OnRecv;
		}

		/// <summary>
		/// accept
		/// </summary>
		public UChannel(USocket socket, UService service) : base(service)
		{
			this.socket = socket;
			this.service = service;
			this.RemoteAddress = socket.RemoteAddress;
			this.socket.Received += this.OnRecv;
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
			this.socket.SendAsync(buffer, channelID, flags);
		}

		public override void Send(List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
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
			TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
			var recvQueue = this.socket.RecvQueue;
			if (recvQueue.Count > 0)
			{
				tcs.SetResult(recvQueue.Dequeue());
			}
			else
			{
				recvTcs = tcs;
			}
			
			return tcs.Task;
		}

		private void OnRecv()
		{
			this.recvTcs?.SetResult(this.socket.RecvQueue.Dequeue());
			this.recvTcs = null;
		}
	}
}