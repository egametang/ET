using System;
using ETModel;

namespace ETHotfix
{
	public abstract class AMHandler<Message> : IMHandler where Message: class
	{
		protected abstract void Run(ETModel.Session session, Message message);

		public void Handle(ETModel.Session session, object msg)
		{
			Message message = msg as Message;
			if (message == null)
			{
				Log.Error($"消息类型转换错误: {msg.GetType().Name} to {typeof(Message).Name}");
			}
			this.Run(session, message);
		}

		public Type GetMessageType()
		{
			return typeof(Message);
		}
	}
}