using System;

namespace Model
{
	public class InnerMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, int offset, byte[] messageBytes, AMessage message)
		{
			// 收到actor消息分发给actor自己去处理
			if (message is ActorRequest actorRequest)
			{
				Entity entity = Game.Scene.GetComponent<ActorManagerComponent>().Get(actorRequest.Id);
				if (entity == null)
				{
					ActorResponse response = new ActorResponse
					{
						RpcId = actorRequest.RpcId,
						Error = ErrorCode.ERR_NotFoundActor
					};
					session.Reply(response);
					return;
				}
				entity.GetComponent<ActorComponent>().Add(new ActorMessageInfo() { Session = session, Message = actorRequest });
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
