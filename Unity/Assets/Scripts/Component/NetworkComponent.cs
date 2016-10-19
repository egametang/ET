using System;
using System.Collections.Generic;
using Base;

namespace Model
{
	[ObjectEvent]
	public class NetworkComponentEvent : ObjectEvent<NetworkComponent>, IUpdate, IAwake<NetworkProtocol>, IAwake<NetworkProtocol, string, int>
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

		public void Awake(NetworkProtocol protocol, string host, int port)
		{
			this.GetValue().Awake(protocol, host, port);
		}
	}

	public class NetworkComponent: Component
	{
		private AService Service;

		private readonly Dictionary<long, Entity> sessions = new Dictionary<long, Entity>();
		private readonly Dictionary<string, Entity> adressSessions = new Dictionary<string, Entity>();

		public void Awake(NetworkProtocol protocol)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.Service = new TService();
					break;
				case NetworkProtocol.UDP:
					this.Service = new UService();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Awake(NetworkProtocol protocol, string host, int port)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.Service = new TService(host, port);
					break;
				case NetworkProtocol.UDP:
					this.Service = new UService(host, port);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			this.StartAccept();
		}

		private async void StartAccept()
		{
			while (true)
			{
				if (this.Id == 0)
				{
					return;
				}

				AChannel channel = await this.Service.AcceptChannel();

				Entity session = new Entity();
				channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
				session.AddComponent<MessageComponent, AChannel>(channel);
				this.Add(session);
			}
		}

		private void Add(Entity session)
		{
			this.sessions.Add(session.Id, session);
			this.adressSessions.Add(session.GetComponent<MessageComponent>().RemoteAddress, session);
		}

		private void Remove(long id)
		{
			Entity session;
			if (!this.sessions.TryGetValue(id, out session))
			{
				return;
			}
			this.sessions.Remove(id);
			this.adressSessions.Remove(session.GetComponent<MessageComponent>().RemoteAddress);
		}

		public Entity Get(long id)
		{
			Entity session;
			this.sessions.TryGetValue(id, out session);
			return session;
		}

		public Entity Get(string address)
		{
			Entity session;
			if (this.adressSessions.TryGetValue(address, out session))
			{
				return session;
			}

			string[] ss = address.Split(':');
			int port = int.Parse(ss[1]);
			string host = ss[0];
			AChannel channel = this.Service.ConnectChannel(host, port);
			session = new Entity();
			channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
			session.AddComponent<MessageComponent, AChannel>(channel);
			this.Add(session);

			return session;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.Service.Dispose();
		}

		public void Update()
		{
			if (this.Service == null)
			{
				return;
			}
			this.Service.Update();
		}
	}
}