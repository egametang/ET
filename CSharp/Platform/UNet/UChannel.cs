using System;
using System.Threading.Tasks;
using Common.Network;

namespace UNet
{
	internal class UChannel: AChannel
	{
		private readonly UService service;
		private USocket socket;
		private readonly string remoteAddress;

		public UChannel(USocket socket, UService service)
		{
			this.socket = socket;
			this.service = service;
			this.remoteAddress = this.socket.RemoteAddress;
		}

		protected void Dispose(bool disposing)
		{
			if (this.socket == null)
			{
				return;
			}

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

		public override void SendAsync(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
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