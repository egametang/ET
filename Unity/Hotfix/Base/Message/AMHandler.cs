using System;
using Model;

namespace Hotfix
{
	public abstract class AMHandler<Message> : IMHandler where Message: AMessage 
	{
		protected abstract void Run(Message message);

		public void Handle(object msg)
		{
			Message message = msg as Message;
			if (message == null)
			{
				Log.Error($"消息类型转换错误: {msg.GetType().Name} to {typeof(Message).Name}");
			}
			this.Run(message);
		}

		public Type GetMessageType()
		{
			return typeof(Message);
		}
	}
}