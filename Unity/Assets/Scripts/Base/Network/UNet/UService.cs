using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
		public UService(IPEndPoint ipEndPoint)
		{
			this.poller = new UPoller(ipEndPoint.Address.ToString(), (ushort)ipEndPoint.Port);
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

		public override AChannel ConnectChannel(IPEndPoint ipEndPoint)
		{
			USocket newSocket = new USocket(this.poller);
			UChannel channel = new UChannel(newSocket, ipEndPoint, this);
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