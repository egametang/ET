using System.Collections.Generic;
using System.Net;

namespace ETModel
{
	public class NetInnerComponent: NetworkComponent
	{
		public readonly Dictionary<IPEndPoint, Session> adressSessions = new Dictionary<IPEndPoint, Session>();

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
		public Session Get(IPEndPoint ipEndPoint)
		{
			if (this.adressSessions.TryGetValue(ipEndPoint, out Session session))
			{
				return session;
			}
			
			session = this.Create(ipEndPoint);

			this.adressSessions.Add(ipEndPoint, session);
			return session;
		}
	}
}