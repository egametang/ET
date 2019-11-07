using System;

namespace ETModel
{
	public class MessageProxy: IMHandler
	{
		private readonly Type type;
		private readonly Action<Session, object> action;

		public MessageProxy(Type type, Action<Session, object> action)
		{
			this.type = type;
			this.action = action;
		}
		
		public async ETVoid Handle(Session session, object message)
		{
			this.action.Invoke(session, message);
		}

		public Type GetMessageType()
		{
			return this.type;
		}
	}
}
