using System;
using System.Net.Sockets;

namespace Base
{
	[ObjectEvent]
	public class NetworkComponentEvent : ObjectEvent<NetworkComponent>, IUpdate, IAwake<NetworkProtocol>
	{
		public void Update()
		{
			NetworkComponent component = this.GetValue();
			component.Update();
		}

		public void Awake(NetworkProtocol protocol)
		{
			this.GetValue().Awake(protocol);
		}
	}

	public class NetworkComponent: Component
	{
		private AService service;
		
		private void Dispose(bool disposing)
		{
			if (this.service == null)
			{
				return;
			}

			base.Dispose();

			if (disposing)
			{
				this.service.Dispose();
			}

			this.service = null;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			this.Dispose(true);
		}

		public void Awake(NetworkProtocol protocol)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.service = new TService { OnError = this.OnError };
					break;
				case NetworkProtocol.UDP:
					this.service = new UService { OnError = this.OnError };
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Update()
		{
			if (this.service == null)
			{
				return;
			}
			this.service.Update();
		}

		public AChannel GetChannel(long channelId)
		{
			AChannel channel = this.service?.GetChannel(channelId);
			return channel;
		}


		public AChannel ConnectChannel(string host, int port)
		{
			AChannel channel = this.service.GetChannel(host, port);
			channel.ConnectAsync();
			return channel;
		}

		public void OnError(long id, SocketError error)
		{
			Env env = new Env();
			env[EnvKey.ChannelError] = error;
			Share.Scene.GetComponent<EventComponent>().Run(EventIdType.NetworkChannelError, env);
		}

		public void RemoveChannel(long channelId)
		{
			AChannel channel = this.service?.GetChannel(channelId);
			if (channel == null)
			{
				return;
			}
			this.service.Remove(channelId);
			channel.Dispose();
		}
	}
}