using System;

namespace Model
{
	public class InnerMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, int offset, byte[] messageBytes, object message)
		{
			// 收到actor消息分发给actor自己去处理
			if (message is IActorMessage actorMessage)
			{
				Entity entity = Game.Scene.GetComponent<ActorManagerComponent>().Get(actorMessage.Id);
				entity.GetComponent<ActorComponent>().Add(new ActorMessageInfo() { Session = session, Message = actorMessage });
				return;
			}

			if (message is AMessage || message is ARequest)
			{
				Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, message);
				return;
			}

			throw new Exception($"message type error: {message.GetType().FullName}");
		}
	}
}
