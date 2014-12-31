using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using Network;

namespace TNet
{
	public class TService: IService
	{
		private readonly IPoller poller = new TPoller();
		private TSocket acceptor;
		
		private readonly Dictionary<string, TChannel> channels = new Dictionary<string, TChannel>();

		private readonly TimerManager timerManager = new TimerManager();

		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public TService(string host, int port)
		{
			this.acceptor = new TSocket(poller);
			this.acceptor.Bind(host, port);
			this.acceptor.Listen(100);
		}

		/// <summary>
		/// 只能做client端的构造函数
		/// </summary>
		public TService()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.acceptor == null)
			{
				return;
			}

			if (disposing)
			{
				this.acceptor.Dispose();
			}

			this.acceptor = null;
		}

		~TService()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Add(Action action)
		{
			this.poller.Add(action);
		}

		private async Task<IChannel> ConnectAsync(string host, int port)
		{
			TSocket newSocket = new TSocket(poller);
			await newSocket.ConnectAsync(host, port);
			TChannel channel = new TChannel(newSocket, this);
			channels[newSocket.RemoteAddress] = channel;
			channel.Start();
			return channel;
		}

		public async Task<IChannel> GetChannel()
		{
			if (this.acceptor == null)
			{
				throw new Exception(string.Format("service construct must use host and port param"));
			}
			TSocket socket = new TSocket(this.poller);
			await acceptor.AcceptAsync(socket);
			TChannel channel = new TChannel(socket, this);
			channels[channel.RemoteAddress] = channel;
			channel.Start();
			return channel;
		}

		public void Remove(IChannel channel)
		{
			TChannel tChannel = channel as TChannel;
			if (tChannel == null)
			{
				return;
			}
			this.channels.Remove(channel.RemoteAddress);
			this.timerManager.Remove(tChannel.SendTimer);
		}

		public async Task<IChannel> GetChannel(string host, int port, uint channelCount)
		{
			TChannel channel = null;
			if (this.channels.TryGetValue(host + ":" + port, out channel))
			{
				return channel;
			}
			return await ConnectAsync(host, port);
		}

		public async Task<IChannel> GetChannel(string address, uint channelCount)
		{
			string[] ss = address.Split(':');
			int port = Convert.ToInt32(ss[1]);
			return await GetChannel(ss[0], port, channelCount);
		}

		public void RunOnce(int timeout)
		{
			poller.Run(timeout);
		}

		public void Run()
		{
			while (true)
			{
				this.RunOnce(1);
				this.timerManager.Refresh();
			}
		}

		internal TimerManager Timer
		{
			get
			{
				return this.timerManager;
			}
		}
	}
}
