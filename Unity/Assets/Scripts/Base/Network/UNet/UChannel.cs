using System;
using System.Collections.Generic;
using System.Linq;

namespace Base
{
	internal class UChannel: AChannel
	{
		private USocket socket;
		private readonly string remoteAddress;

		public UChannel(USocket socket, string host, int port, UService service): base(service)
		{
			this.socket = socket;
			this.service = service;
			this.remoteAddress = host + ":" + port;
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

		public override void ConnectAsync()
		{
			string[] ss = this.remoteAddress.Split(':');
			ushort port = ushort.Parse(ss[1]);
			this.socket.ConnectAsync(ss[0], port);
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

		public override byte[] Recv()
		{
			if (this.socket?.RecvQueue.Count == 0)
			{
				return null;
			}
			return this.socket?.RecvQueue.Dequeue();
		}
	}
}