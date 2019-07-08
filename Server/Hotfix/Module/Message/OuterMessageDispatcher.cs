using ETModel;

namespace ETHotfix
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, object message)
		{
			DispatchAsync(session, opcode, message).Coroutine();
		}
		
		public async ETVoid DispatchAsync(Session session, ushort opcode, object message)
		{
			// 根据消息接口判断是不是Actor消息，不同的接口做不同的处理
			switch (message)
			{
				case IActorLocationRequest actorLocationRequest: // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
				{
					long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
					ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);

					int rpcId = actorLocationRequest.RpcId; // 这里要保存客户端的rpcId
					long instanceId = session.InstanceId;
					IResponse response = await actorLocationSender.Call(actorLocationRequest);
					response.RpcId = rpcId;

					// session可能已经断开了，所以这里需要判断
					if (session.InstanceId == instanceId)
					{
						session.Reply(response);
					}
					
					break;
				}
				case IActorLocationMessage actorLocationMessage:
				{
					long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
					ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);
					actorLocationSender.Send(actorLocationMessage);
					break;
				}
				case IActorRequest actorRequest:  // 分发IActorRequest消息，目前没有用到，需要的自己添加
				{
					break;
				}
				case IActorMessage actorMessage:  // 分发IActorMessage消息，目前没有用到，需要的自己添加
				{
					break;
				}
				default:
				{
					// 非Actor消息
					Game.Scene.GetComponent<MessageDispatcherComponent>().Handle(session, new MessageInfo(opcode, message));
					break;
				}
			}
		}
	}
}
