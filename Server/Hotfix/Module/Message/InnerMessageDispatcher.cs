using System;
using ETModel;

namespace ETHotfix
{
	public class InnerMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, Packet packet)
		{
			ushort opcode = packet.Opcode();
			Type messageType = Game.Scene.GetComponent<OpcodeTypeComponent>().GetType(opcode);
			IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(messageType, packet.Bytes, Packet.Index, packet.Length - Packet.Index);
			
			// 收到actor消息,放入actor队列
			if (message is IActorMessage iActorMessage)
			{
				Entity entity = (Entity)Game.EventSystem.Get(iActorMessage.ActorId);
				if (entity == null)
				{
					Log.Warning($"not found actor: {iActorMessage.ActorId}");
					ActorResponse response = new ActorResponse
					{
						Error = ErrorCode.ERR_NotFoundActor,
						RpcId = iActorMessage.RpcId
					};
					session.Reply(response);
					return;
				}
				
				entity.GetComponent<ActorComponent>().Add(new ActorMessageInfo() { Session = session, Message = iActorMessage });
				return;
			}
			
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(opcode, message));
		}
	}
}
