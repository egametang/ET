using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Network;

namespace UNet
{
	public sealed class UService: IService
	{
		private EService service;

		private readonly Dictionary<string, UChannel> channels = new Dictionary<string, UChannel>();

		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public UService(string host, int port)
		{
			this.service = new EService(host, (ushort)port);
		}

		/// <summary>
		/// 只能做client
		/// </summary>
		public UService()
		{
			this.service = new EService();
		}

		private void Dispose(bool disposing)
		{
			if (service == null)
			{
				return;
			}

			if (disposing)
			{
				service.Dispose();	
			}
			service = null;
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
			this.service.Events += action;
		}

		public EService Service
		{
			get
			{
				return service;
			}
		}

		private async Task<IChannel> ConnectAsync(string host, int port)
		{
			USocket newSocket = new USocket(service);
			await newSocket.ConnectAsync(host, (ushort) port);
			UChannel channel = new UChannel(newSocket, this);
			channels[channel.RemoteAddress] = channel;
			return channel;
		}

		public async Task<IChannel> GetChannel(string address)
		{
			string[] ss = address.Split(':');
			int port = Convert.ToInt32(ss[1]);
			return await GetChannel(ss[0], port);
		}

		public async Task<IChannel> GetChannel()
		{
			USocket socket = new USocket(this.service);
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

		public async Task<IChannel> GetChannel(string host, int port)
		{
			UChannel channel = null;
			if (this.channels.TryGetValue(host + ":" + port, out channel))
			{
				return channel;
			}
			return await ConnectAsync(host, port);
		}

		public void RunOnce(int timeout)
		{
			this.service.RunOnce(timeout);
		}

		public void Run()
		{
			this.service.Run();
		}
	}
}
