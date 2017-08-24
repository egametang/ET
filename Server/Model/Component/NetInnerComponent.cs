using System.Collections.Generic;

namespace Model
{
	[ObjectEvent]
	public class NetInnerComponentEvent : ObjectEvent<NetInnerComponent>, IAwake, IAwake<string, int>, IUpdate
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Awake(string a, int b)
		{
			this.Get().Awake(a, b);
		}

		public void Update()
		{
			this.Get().Update();
		}
	}
	
	public class NetInnerComponent: NetworkComponent
	{
		private readonly Dictionary<string, Session> adressSessions = new Dictionary<string, Session>();

		public void Awake()
		{
			this.Awake(NetworkProtocol.TCP);
			this.MessagePacker = new MongoPacker();
			this.MessageDispatcher = new InnerMessageDispatcher();
		}

		public void Awake(string host, int port)
		{
			this.Awake(NetworkProtocol.TCP, host, port);
			this.MessagePacker = new MongoPacker();
			this.MessageDispatcher = new InnerMessageDispatcher();
		}

		public new void Update()
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