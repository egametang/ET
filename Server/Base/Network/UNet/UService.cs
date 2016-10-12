using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Base
{
	public sealed class UService: AService
	{
		private UPoller poller;
		
		private readonly Dictionary<long, UChannel> idChannels = new Dictionary<long, UChannel>();

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
				foreach (long id in this.idChannels.Keys.ToArray())
				{
					UChannel channel = this.idChannels[id];
					channel.Dispose();
				}
				this.poller.Dispose();
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

		public override AChannel GetChannel(string host, int port)
		{
			UChannel channel = null;

			USocket newSocket = new USocket(this.poller);
			channel = new UChannel(newSocket, host, port, this);
			newSocket.Disconnect += () => this.OnChannelError(channel.Id, SocketError.SocketError);
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override AChannel GetChannel(string address)
		{
			string[] ss = address.Split(':');
			int port = int.Parse(ss[1]);
			return this.GetChannel(ss[0], port);
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