using ETModel;

namespace ETHotfix
{
	public class InnerMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, object message)
		{
			// 收到actor消息,放入actor队列
			switch (message)
			{
				case IActorRequest iActorRequest:
				{
					Entity entity = (Entity)Game.EventSystem.Get(iActorRequest.ActorId);
					if (entity == null)
					{
						Log.Warning($"not found actor: {message}");
						ActorResponse response = new ActorResponse
						{
							Error = ErrorCode.ERR_NotFoundActor,
							RpcId = iActorRequest.RpcId
						};
						session.Reply(response);
						return;
					}
	
					MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
					if (mailBoxComponent == null)
					{
						ActorResponse response = new ActorResponse
						{
							Error = ErrorCode.ERR_ActorNoMailBoxComponent,
							RpcId = iActorRequest.RpcId
						};
						session.Reply(response);
						Log.Error($"actor not add MailBoxComponent: {entity.GetType().Name} {message}");
						return;
					}
				
					mailBoxComponent.Add(new ActorMessageInfo() { Session = session, Message = iActorRequest });
					return;
				}
				case IActorMessage iactorMessage:
				{
					Entity entity = (Entity)Game.EventSystem.Get(iactorMessage.ActorId);
					if (entity == null)
					{
						Log.Error($"not found actor: {message}");
						return;
					}
	
					MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
					if (mailBoxComponent == null)
					{
						Log.Error($"actor not add MailBoxComponent: {entity.GetType().Name} {message}");
						return;
					}
				
					mailBoxComponent.Add(new ActorMessageInfo() { Session = session, Message = iactorMessage });
					return;
				}
				default:
				{
					Game.Scene.GetComponent<MessageDispatcherComponent>().Handle(session, new MessageInfo(opcode, message));
					break;
				}
			}
		}
	}
}
