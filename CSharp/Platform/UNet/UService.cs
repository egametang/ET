using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Base;
using Common.Logger;
using Common.Network;
using MongoDB.Bson;

namespace UNet
{
	public sealed class UService: IService
	{
		private UPoller poller;

		private readonly Dictionary<string, UChannel> channels = new Dictionary<string, UChannel>();

		private readonly Dictionary<ObjectId, UChannel> idChannels = new Dictionary<ObjectId, UChannel>();

		private readonly TimerManager timerManager = new TimerManager();

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
				foreach (ObjectId id in this.idChannels.Keys.ToArray())
				{
					UChannel channel = this.idChannels[id];
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

		private async void SocketConnectAsync(AChannel channel)
		{
			while (true)
			{
				try
				{
					await channel.ConnectAsync();
					break;
				}
				catch (Exception e)
				{
					Log.Trace(e.ToString());
				}

				await this.Timer.Sleep(5000);
			}
		}

		public AChannel GetChannel(string host, int port)
		{
			UChannel channel = null;
			if (this.channels.TryGetValue(host + ":" + port, out channel))
			{
				return channel;
			}

			USocket newSocket = new USocket(this.poller);
			channel = new UChannel(newSocket, host, port, this);
			this.channels[channel.RemoteAddress] = channel;
			this.idChannels[channel.Id] = channel;
			this.SocketConnectAsync(channel);
			return channel;
		}

		public AChannel GetChannel(string address)
		{
			string[] ss = address.Split(':');
			int port = int.Parse(ss[1]);
			return this.GetChannel(ss[0], port);
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

		public void Update()
		{
			this.poller.Update();
		}

		public TimerManager Timer
		{
			get
			{
				return this.timerManager;
			}
		}
	}
}