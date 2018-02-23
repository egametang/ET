using System;

namespace Model
{
	public class MessageProxy: IMHandler
	{
		private readonly Type type;
		private readonly Action<Session, uint, object> action;

		public MessageProxy(Type type, Action<Session, uint, object> action)
		{
			this.type = type;
			this.action = action;
		}
		
		public void Handle(Session session, uint rpcId, object message)
		{
			this.action.Invoke(session, rpcId, message);
		}

		public Type GetMessageType()
		{
			return this.type;
		}
	}
}
