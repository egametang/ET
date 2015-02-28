using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Network;
using MongoDB.Bson;

namespace UNet
{
	public sealed class UService: IService
	{
		private UPoller poller;

		private readonly Dictionary<string, UChannel> channels = new Dictionary<string, UChannel>();

		private readonly Dictionary<ObjectId, UChannel> idChannels = new Dictionary<ObjectId, UChannel>();

		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public UService(string host, int port)
		{
			this.poller = new UPoller(host, (ushort) port);
		}

		/// <summary>
		/// 只能做client
		/// </summary>
		public UService()
		{
			this.poller = new UPoller();
		}

		private void Dispose(bool disposing)
		{
			if (this.poller == null)
			{
				return;
			}

			if (disposing)
			{
				foreach (ObjectId id in idChannels.Keys.ToArray())
				{
					UChannel channel = idChannels[id];
					channel.Dispose();
				}
				this.poller.Dispose();
			}

			this.poller = null;
		}

		~UService()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Add(Action action)
		{
			this.poller.Add(action);
		}

		private async Task<AChannel> ConnectAsync(string host, int port)
		{
			USocket newSocket = await this.poller.ConnectAsync(host, (ushort) port);
			UChannel channel = new UChannel(newSocket, this);
			this.channels[channel.RemoteAddress] = channel;
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public async Task<AChannel> GetChannel(string host, int port)
		{
			UChannel channel = null;
			if (this.channels.TryGetValue(host + ":" + port, out channel))
			{
				return channel;
			}
			return await this.ConnectAsync(host, port);
		}

		public async Task<AChannel> GetChannel()
		{
			USocket socket = await this.poller.AcceptAsync();
			UChannel channel = new UChannel(socket, this);
			this.channels[channel.RemoteAddress] = channel;
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public AChannel GetChannel(ObjectId id)
		{
			UChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

		public void Remove(AChannel channel)
		{
			UChannel tChannel = channel as UChannel;
			if (tChannel == null)
			{
				return;
			}
			this.idChannels.Remove(channel.Id);
			this.channels.Remove(channel.RemoteAddress);
		}

		public void Run()
		{
			this.poller.RunOnce();
		}
	}
}