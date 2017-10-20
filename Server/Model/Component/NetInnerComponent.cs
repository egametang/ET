using System.Collections.Generic;

namespace Model
{
	public class NetInnerComponent: NetworkComponent
	{
		public readonly Dictionary<string, Session> adressSessions = new Dictionary<string, Session>();

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
			if (this.adressSessions.TryGetValue(address, out Session session))
			{
				return session;
			}

			session = this.Create(address);
			this.adressSessions.Add(address, session);
			return session;
		}
	}
}