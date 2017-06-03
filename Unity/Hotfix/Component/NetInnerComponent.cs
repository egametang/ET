using System.Collections.Generic;
using Model;

namespace Hotfix
{
	[EntityEvent(EntityEventId.NetInnerComponent)]
	public class NetInnerComponent: NetworkComponent
	{
		private readonly Dictionary<string, Session> adressSessions = new Dictionary<string, Session>();

		private void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
		}

		private void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
		}

		private new void Update()
		{
			base.Update();
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