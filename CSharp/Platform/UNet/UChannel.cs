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

		public void Dispose()
		{
			if (socket == null)
			{
				return;
			}
			service.Remove(this);
			socket.Dispose();
			this.socket = null;
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
