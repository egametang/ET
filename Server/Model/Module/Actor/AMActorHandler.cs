using System;

namespace ETModel
{
	public abstract class AMActorHandler<E, Message>: IMActorHandler where E: Entity where Message : class, IActorMessage
	{
		protected abstract ETTask Run(E entity, Message message);

		public async ETTask Handle(Session session, Entity entity, object actorMessage)
		{
			Message msg = actorMessage as Message;
			if (msg == null)
			{
				Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Message).Name}");
				return;
			}
			E e = entity as E;
			if (e == null)
			{
				Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof(E).Name}");
				return;
			}

			try
			{
				await this.Run(e, msg);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public Type GetMessageType()
		{
			return typeof (Message);
		}
	}
}