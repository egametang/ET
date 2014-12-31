using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Network;

namespace UNet
{
	public sealed class UService: IService
	{
		private UPoller poller;

		private readonly Dictionary<string, UChannel> channels = new Dictionary<string, UChannel>();

		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
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

		private void Dispose(bool disposing)
		{
			if (this.poller == null)
			{
				return;
			}

			if (disposing)
			{
				this.poller.Dispose();	
			}
			this.poller = null;
		}

		~UService()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Add(Action action)
		{
			this.poller.Events += action;
		}

		public UPoller Poller
		{
			get
			{
				return this.poller;
			}
		}

		private async Task<IChannel> ConnectAsync(string host, int port, uint channelCount)
		{
			USocket newSocket = new USocket(this.poller);
			await newSocket.ConnectAsync(host, (ushort)port, channelCount);
			UChannel channel = new UChannel(newSocket, this);
			channels[channel.RemoteAddress] = channel;
			return channel;
		}

		public async Task<IChannel> GetChannel(string address, uint channelCount)
		{
			string[] ss = address.Split(':');
			int port = Convert.ToInt32(ss[1]);
			return await GetChannel(ss[0], port, channelCount);
		}

		public async Task<IChannel> GetChannel(string host, int port, uint channelCount)
		{
			UChannel channel = null;
			if (this.channels.TryGetValue(host + ":" + port, out channel))
			{
				return channel;
			}
			return await ConnectAsync(host, port, channelCount);
		}

		public async Task<IChannel> GetChannel()
		{
			USocket socket = new USocket(this.poller);
			await socket.AcceptAsync();
			UChannel channel = new UChannel(socket, this);
			channels[channel.RemoteAddress] = channel;
			return channel;
		}

		public void Remove(IChannel channel)
		{
			UChannel tChannel = channel as UChannel;
			if (tChannel == null)
			{
				return;
			}
			this.channels.Remove(channel.RemoteAddress);
		}

		public void RunOnce(int timeout)
		{
			this.poller.RunOnce(timeout);
		}

		public void Run()
		{
			this.poller.Run();
		}
	}
}
