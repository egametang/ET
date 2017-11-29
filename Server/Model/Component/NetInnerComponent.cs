using System.Collections.Generic;
using System.Net;

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
			this.adressSessions.Remove(session.RemoteAddress.ToString());

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

			string[] ss = address.Split(':');
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ss[0]), int.Parse(ss[1]));
			session = this.Create(ipEndPoint);

			this.adressSessions.Add(address, session);
			return session;
		}
	}
}