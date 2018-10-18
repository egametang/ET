using System;
using System.Threading.Tasks;

namespace ETModel
{
	public abstract class AMActorHandler<E, Message>: IMActorHandler where E: Entity where Message : class, IActorMessage
	{
		protected abstract void Run(E entity, Message message);

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

			this.Run(e, msg);

			await ETTask.CompletedTask;
		}

		public Type GetMessageType()
		{
			return typeof (Message);
		}
	}
}