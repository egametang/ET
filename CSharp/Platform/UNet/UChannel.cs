using System;
using System.Threading.Tasks;
using Network;

namespace UNet
{
	internal class UChannel: IChannel
	{
		private readonly UService service;
		private USocket socket;


		public UChannel(USocket socket, UService service)
		{
			this.socket = socket;
			this.service = service;
		}

		protected void Dispose(bool disposing)
		{
			if (socket == null)
			{
				return;
			}
			
			if (disposing)
			{
				socket.Dispose();
			}

			service.Remove(this);
			this.socket = null;
		}

		~UChannel()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void SendAsync(byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable)
		{
			this.socket.WriteAsync(buffer, channelID, flags);
		}


		public async Task<byte[]> RecvAsync()
		{
			return await this.socket.ReadAsync();
		}

		public string RemoteAddress
		{
			get
			{
				return this.socket.RemoteAddress;
			}
		}

		public async Task<bool> DisconnnectAsync()
		{
			return await this.socket.DisconnectAsync();
		}
	}
}
