using System;

namespace ETModel
{
	public abstract class AMHandler<Message> : IMHandler where Message: class
	{
		protected abstract ETTask Run(Session session, Message message);

		public async ETVoid Handle(Session session, object msg)
		{
			Message message = msg as Message;
			if (message == null)
			{
				Log.Error($"消息类型转换错误: {msg.GetType().Name} to {typeof(Message).Name}");
				return;
			}
			if (session.IsDisposed)
			{
				Log.Error($"session disconnect {msg}");
				return;
			}

			try
			{
				await this.Run(session, message);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public Type GetMessageType()
		{
			return typeof(Message);
		}
	}
}