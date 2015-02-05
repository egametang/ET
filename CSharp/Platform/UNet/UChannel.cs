using System;
using System.Threading.Tasks;
using Network;

namespace UNet
{
	internal class UChannel: IChannel
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

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void SendAsync(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			this.socket.SendAsync(buffer, channelID, flags);
		}

		public async Task<byte[]> RecvAsync()
		{
			return await this.socket.RecvAsync();
		}

		public string RemoteAddress
		{
			get
			{
				return this.remoteAddress;
			}
		}

		public async Task<bool> DisconnnectAsync()
		{
			return await this.socket.DisconnectAsync();
		}
	}
}