using System;

namespace Model
{
	public abstract class AMHandler<Message> : IMHandler where Message : MessageObject
	{
		protected abstract void Run(Session session, Message message);

		public void Handle(Session session, object msg)
		{
			Message message = msg as Message;
			if (message == null)
			{
				Log.Error($"消息类型转换错误: {msg.GetType().Name} to {typeof(Message).Name}");
				return;
			}
			if (session.Id == 0)
			{
				Log.Error($"session disconnect {msg}");
				return;
			}
			this.Run(session, message);
		}

		public Type GetMessageType()
		{
			return typeof(Message);
		}
	}
}