using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
	public sealed class UService: AService
	{
		private UPoller poller;

		private readonly Dictionary<long, UChannel> idChannels = new Dictionary<long, UChannel>();

		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		public UService(string host, int port)
		{
			this.poller = new UPoller(host, (ushort)port);
		}

		/// <summary>
		/// 只能做client
		/// </summary>
		public UService()
		{
			this.poller = new UPoller();
		}

		public override void Dispose()
		{
			if (this.poller == null)
			{
				return;
			}

			foreach (long id in this.idChannels.Keys.ToArray())
			{
				UChannel channel = this.idChannels[id];
				channel.Dispose();
			}
			
			this.poller = null;
		}
		
		public override async Task<AChannel> AcceptChannel()
		{
			USocket socket = await this.poller.AcceptAsync();
			UChannel channel = new UChannel(socket, this);
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override AChannel ConnectChannel(string host, int port)
		{
			USocket newSocket = new USocket(this.poller);
			UChannel channel = new UChannel(newSocket, host, port, this);
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override AChannel GetChannel(long id)
		{
			UChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

		public override void Remove(long id)
		{
			UChannel channel;
			if (!this.idChannels.TryGetValue(id, out channel))
			{
				return;
			}
			if (channel == null)
			{
				return;
			}
			this.idChannels.Remove(id);
			channel.Dispose();
		}

		public override void Update()
		{
			this.poller.Update();
		}
	}
}