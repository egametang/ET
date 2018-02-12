using System;
using Model;

namespace Hotfix
{
	public class InnerMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, PacketInfo packetInfo)
		{
			Type messageType = Game.Scene.GetComponent<OpcodeTypeComponent>().GetType(packetInfo.Opcode);
			IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(messageType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
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
					session.Reply(packetInfo.RpcId, response);
					return;
				}
				
				entity.GetComponent<ActorComponent>().Add(new ActorMessageInfo() { Session = session, RpcId = packetInfo.RpcId, Message = actorRpcRequest });
				return;
			}
			
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, packetInfo.RpcId, message);
		}
	}
}
