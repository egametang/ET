using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Base
{
	public sealed class TService: AService
	{
		private TPoller poller = new TPoller();
		private readonly TSocket acceptor;

		private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();
		private readonly Dictionary<string, TChannel> addressChannels = new Dictionary<string, TChannel>();

		/// <summary>
		/// 即可做client也可做server
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public TService(string host, int port)
		{
			this.acceptor = new TSocket(this.poller, host, port);
		}

		public TService()
		{
		}

		public override void Dispose()
		{
			if (this.poller == null)
			{
				return;
			}

			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TChannel channel = this.idChannels[id];
				channel.Dispose();
			}

			this.poller = null;
		}

		public override void Add(Action action)
		{
			this.poller.Add(action);
		}

		public override AChannel GetChannel(long id)
		{
			TChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}

		public override async Task<AChannel> AcceptChannel()
		{
			if (this.acceptor == null)
			{
				throw new Exception("service construct must use host and port param");
			}
			TSocket socket = new TSocket(this.poller);
			await this.acceptor.AcceptAsync(socket);
			TChannel channel = new TChannel(socket, this);
			this.addressChannels[channel.RemoteAddress] = channel;
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override void Remove(long id)
		{
			TChannel channel;
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

		public override AChannel GetChannel(string host, int port)
		{
			string address = $"{host}:{port}";
			return this.GetChannel(address);
		}

		public override AChannel GetChannel(string address)
		{
			TChannel channel = null;
			if (this.addressChannels.TryGetValue(address, out channel))
			{
				return channel;
			}

			string[] ss = address.Split(':');
			string host = ss[0];
			int port = int.Parse(ss[1]);
			TSocket newSocket = new TSocket(this.poller);
			channel = new TChannel(newSocket, host, port, this);
			channel.OnError += this.OnChannelError;
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override void Update()
		{
			this.poller.Update();
		}
	}
}