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
			// 收到actor rpc request
			if (message is ActorRequest actorRpcRequest)
			{
				Entity entity = Game.Scene.GetComponent<ActorManagerComponent>().Get(actorRpcRequest.Id);
				if (entity == null)
				{
					Log.Warning($"not found actor: {actorRpcRequest.Id}");
					ActorResponse response = new ActorResponse
					{
						Error = ErrorCode.ERR_NotFoundActor
					};
					session.Reply(response);
					return;
				}
				
				entity.GetComponent<ActorComponent>().Add(new ActorMessageInfo() { Session = session, RpcId = actorRpcRequest.RpcId, Message = actorRpcRequest });
				return;
			}
			
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(opcode, message));
		}
	}
}
