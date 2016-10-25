using System;
using System.Collections.Generic;
using System.Linq;
using Base;

namespace Model
{
	public abstract class NetworkComponent: Component
	{
		private AService Service;

		private readonly Dictionary<long, Session> sessions = new Dictionary<long, Session>();
		private readonly Dictionary<string, Session> adressSessions = new Dictionary<string, Session>();

		protected void Awake(NetworkProtocol protocol)
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

		protected void Awake(NetworkProtocol protocol, string host, int port)
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

				Session session = new Session(channel);
				channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
				this.Add(session);
			}
		}

		private void Add(Session session)
		{
			this.sessions.Add(session.Id, session);
			this.adressSessions.Add(session.RemoteAddress, session);
		}

		public void Remove(long id)
		{
			Session session;
			if (!this.sessions.TryGetValue(id, out session))
			{
				return;
			}
			this.sessions.Remove(id);
			this.adressSessions.Remove(session.RemoteAddress);
		}

		public Session Get(long id)
		{
			Session session;
			this.sessions.TryGetValue(id, out session);
			return session;
		}

		public Session Get(string address)
		{
			Session session;
			if (this.adressSessions.TryGetValue(address, out session))
			{
				return session;
			}

			string[] ss = address.Split(':');
			int port = int.Parse(ss[1]);
			string host = ss[0];
			AChannel channel = this.Service.ConnectChannel(host, port);
			session = new Session(channel);
			channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
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

			foreach (Session session in this.sessions.Values.ToArray())
			{
				session.Dispose();
			}
			
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