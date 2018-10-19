using System;
using ETModel;
using Google.Protobuf;

namespace ETHotfix
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, object message)
		{
			DispatchAsync(session, opcode, message).NoAwait();
		}
		
		public async ETVoid DispatchAsync(Session session, ushort opcode, object message)
		{
			try
			{
				switch (message)
				{
					case IActorLocationRequest actorLocationRequest: // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
					{
						long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
						ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);

						int rpcId = actorLocationRequest.RpcId; // 这里要保存客户端的rpcId
						IResponse response = await actorLocationSender.Call(actorLocationRequest);
						response.RpcId = rpcId;

						session.Reply(response);
						return;
					}
					case IActorLocationMessage actorLocationMessage:
					{
						long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
						ActorLocationSender actorLocationSender = Game.Scene.GetComponent<ActorLocationSenderComponent>().Get(unitId);
						actorLocationSender.Send(actorLocationMessage);
						return;
					}
				}

				Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(opcode, message));
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
