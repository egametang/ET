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

		private event Action<Session> removeCallback;

		public event Action<Session> RemoveCallback
		{
			add
			{
				this.removeCallback += value;
			}
			remove
			{
				this.removeCallback -= value;
			}
		}

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

				Session session = new Session(this.GetOwner<Scene>(), channel);
				channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
				this.Add(session);
			}
		}

		private void Add(Session session)
		{
			this.sessions.Add(session.Id, session);
			this.AddToAddressDict(session);
		}

		private void AddToAddressDict(Session session)
		{
			Session s;
			if (this.adressSessions.TryGetValue(session.RemoteAddress, out s))
			{
				this.Remove(s.Id);
				Log.Warning($"session 地址冲突, 可能是客户端断开, 服务器还没检测到!: {session.RemoteAddress}");
			}
			this.adressSessions.Add(session.RemoteAddress, session);
		}

		public void Remove(long id)
		{
			Session session;
			if (!this.sessions.TryGetValue(id, out session))
			{
				return;
			}
			removeCallback.Invoke(session);
			this.sessions.Remove(id);
			this.adressSessions.Remove(session.RemoteAddress);
			session.Dispose();
		}

		public Session Get(long id)
		{
			Session session;
			this.sessions.TryGetValue(id, out session);
			return session;
		}

		/// <summary>
		/// 从地址缓存中取Session,如果没有则创建一个新的Session,并且保存到地址缓存中
		/// </summary>
		public Session Get(string address)
		{
			Session session;
			if (this.adressSessions.TryGetValue(address, out session))
			{
				return session;
			}

			session = this.GetNew(address);
			this.AddToAddressDict(session);
			return session;
		}

		/// <summary>
		/// 创建一个新Session,不保存到地址缓存中
		/// </summary>
		public Session GetNew(string address)
		{
			string[] ss = address.Split(':');
			int port = int.Parse(ss[1]);
			string host = ss[0];
			AChannel channel = this.Service.ConnectChannel(host, port);
			Session session = new Session(this.GetOwner<Scene>(), channel);
			channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
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