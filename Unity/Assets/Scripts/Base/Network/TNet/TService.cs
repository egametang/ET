using System;
using System.Collections.Generic;
using System.Linq;

namespace Base
{
	public sealed class TService: AService
	{
		private TPoller poller = new TPoller();
		
		private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();
		
		private void Dispose(bool disposing)
		{
			if (this.poller == null)
			{
				return;
			}

			if (disposing)
			{
				foreach (long id in this.idChannels.Keys.ToArray())
				{
					TChannel channel = this.idChannels[id];
					channel.Dispose();
				}
			}

			this.poller = null;
		}

		public override void Dispose()
		{
			this.Dispose(true);
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
			TChannel channel = null;
			TSocket newSocket = new TSocket(this.poller);
			channel = new TChannel(newSocket, host, port, this);
			channel.OnError += this.OnChannelError;
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override AChannel GetChannel(string address)
		{
			string[] ss = address.Split(':');
			int port = int.Parse(ss[1]);
			return this.GetChannel(ss[0], port);
		}

		public override void Update()
		{
			this.poller.Update();
		}
	}
}