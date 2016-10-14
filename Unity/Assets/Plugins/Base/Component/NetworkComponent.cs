using System;
using System.Net.Sockets;

namespace Base
{
	[ObjectEvent]
	public class NetworkComponentEvent : ObjectEvent<NetworkComponent>, IUpdate, IAwake<NetworkProtocol, string, int>
	{
		public void Update()
		{
			NetworkComponent component = this.GetValue();
			component.Update();
		}

		public void Awake(NetworkProtocol protocol, string host, int port)
		{
			this.GetValue().Awake(protocol, host, port);
		}
	}

	public class NetworkComponent: Component
	{
		public AService Service;
		
		private void Dispose(bool disposing)
		{
			if (this.Service == null)
			{
				return;
			}

			base.Dispose();

			if (disposing)
			{
				this.Service.Dispose();
			}

			this.Service = null;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			this.Dispose(true);
		}

		public void Awake(NetworkProtocol protocol, string host, int port)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.Service = new TService(host, port) { OnError = this.OnError };
					break;
				case NetworkProtocol.UDP:
					this.Service = new UService(host, port) { OnError = this.OnError };
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Update()
		{
			if (this.Service == null)
			{
				return;
			}
			this.Service.Update();
		}
		
		public void OnError(long id, SocketError error)
		{
			Env env = new Env();
			env[EnvBaseKey.ChannelError] = error;
			Game.Scene.GetComponent<EventComponent>().Run(EventBaseType.NetworkChannelError, env);
		}
	}
}