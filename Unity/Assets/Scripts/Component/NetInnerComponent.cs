using System.Collections.Generic;
using Base;

namespace Model
{
	[ObjectEvent]
	public class NetInnerComponentEvent : ObjectEvent<NetInnerComponent>, IUpdate, IAwake, IAwake<string, int>
	{
		public void Update()
		{
			NetworkComponent component = this.GetValue();
			component.Update();
		}

		public void Awake()
		{
			this.GetValue().Awake();
		}

		public void Awake(string host, int port)
		{
			this.GetValue().Awake(host, port);
		}
	}

	public class NetInnerComponent : NetworkComponent
	{
		private readonly Dictionary<string, Session> adressSessions = new Dictionary<string, Session>();

		public void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
		}

		public override void Remove(long id)
		{
			Session session = this.Get(id);
			if (session == null)
			{
				return;
			}
			this.adressSessions.Remove(session.RemoteAddress);

			base.Remove(id);
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

			session = this.Create(address);
			this.adressSessions.Add(address, session);
			return session;
		}
	}
}