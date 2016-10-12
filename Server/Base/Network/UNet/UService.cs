using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Base
{
	public sealed class UService: AService
	{
		private UPoller poller;

		private readonly Dictionary<long, UChannel> idChannels = new Dictionary<long, UChannel>();
		private readonly Dictionary<string, UChannel> addressChannels = new Dictionary<string, UChannel>();

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

		public override void Add(Action action)
		{
			this.poller.Add(action);
		}

		public override AChannel GetChannel(string host, int port)
		{
			return this.GetChannel($"{host}:{port}");
		}

		public override AChannel GetChannel(string address)
		{
			UChannel channel = null;
			if (this.addressChannels.TryGetValue(address, out channel))
			{
				return channel;
			}
			USocket newSocket = new USocket(this.poller);
			string[] ss = address.Split(':');
			int port = int.Parse(ss[1]);
			string host = ss[0];
			channel = new UChannel(newSocket, host, port, this);
			newSocket.Disconnect += () => this.OnChannelError(channel.Id, SocketError.SocketError);
			this.idChannels[channel.Id] = channel;
			return channel;
		}

		public override async Task<AChannel> GetChannel()
		{
			USocket socket = await this.poller.AcceptAsync();
			UChannel channel = new UChannel(socket, this);
			this.addressChannels[channel.RemoteAddress] = channel;
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