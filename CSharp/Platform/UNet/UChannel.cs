using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Helper;
using Common.Network;
using MongoDB.Bson;

namespace UNet
{
	internal class UChannel: AChannel
	{
		private USocket socket;
		private readonly string remoteAddress;

		public UChannel(USocket socket, UService service): base(service)
		{
			this.socket = socket;
			this.service = service;
			this.remoteAddress = this.socket.RemoteAddress;
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

		public override void SendAsync(
				byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
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