using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logger;
using Common.Network;

namespace UNet
{
	internal class BufferInfo
	{
		public byte[] Buffer { get; set; }
		public byte ChannelID { get; set; }
		public PacketFlags Flags { get; set; }
	}

	internal class UChannel: AChannel
	{
		private USocket socket;
		private readonly string remoteAddress;
		private readonly Queue<BufferInfo> queue = new Queue<BufferInfo>();
		private bool isConnected;

		public UChannel(USocket socket, UService service): base(service)
		{
			this.isConnected = true;
			this.socket = socket;
			this.service = service;
			this.remoteAddress = this.socket.RemoteAddress;
		}

		public UChannel(USocket socket, string host, int port, UService service)
			: base(service)
		{
			this.socket = socket;
			this.service = service;
			this.remoteAddress = host + ":" + port;
		}

		private void Dispose(bool disposing)
		{
			if (this.socket == null)
			{
				return;
			}

			this.onDispose(this);

			if (disposing)
			{
				this.socket.Dispose();
			}

			this.service.Remove(this);

			this.socket = null;
		}

		~UChannel()
		{
			this.Dispose(false);
		}

		public override void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override async Task<bool> ConnectAsync()
		{
			string[] ss = this.RemoteAddress.Split(':');
			int port = int.Parse(ss[1]);
			bool result = await this.socket.ConnectAsync(ss[0], (ushort)port);
			this.isConnected = true;
			while (this.queue.Count > 0)
			{
				BufferInfo info = this.queue.Dequeue();
				this.SendAsync(info.Buffer, info.ChannelID, info.Flags);
			}
			return result;
		}

		public override void SendAsync(
				byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			if (!this.isConnected)
			{
				BufferInfo info = new BufferInfo { Buffer = buffer, ChannelID = channelID, Flags = flags };
				this.queue.Enqueue(info);
				return;
			}
			this.socket.SendAsync(buffer, channelID, flags);
		}

		public override void SendAsync(
				List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			int size = buffers.Select(b => b.Length).Sum();
			byte[] buffer = new byte[size];
			int index = 0;
			foreach (byte[] bytes in buffers)
			{
				Array.Copy(bytes, 0, buffer, index, bytes.Length);
				index += bytes.Length;
			}

			if (!this.isConnected)
			{
				BufferInfo info = new BufferInfo { Buffer = buffer, ChannelID = channelID, Flags = flags };
				this.queue.Enqueue(info);
				return;
			}

			this.socket.SendAsync(buffer, channelID, flags);
		}

		public override async Task<byte[]> RecvAsync()
		{
			return await this.socket.RecvAsync();
		}

		public override string RemoteAddress
		{
			get
			{
				return this.remoteAddress;
			}
		}

		public override async Task<bool> DisconnnectAsync()
		{
			return await this.socket.DisconnectAsync();
		}
	}
}