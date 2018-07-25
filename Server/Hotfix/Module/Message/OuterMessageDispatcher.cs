using System;
using ETModel;
using Google.Protobuf;

namespace ETHotfix
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public async void Dispatch(Session session, Packet packet)
		{
			object message;
			try
			{
				OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
				object instance = opcodeTypeComponent.GetInstance(packet.Opcode);
				message = session.Network.MessagePacker.DeserializeFrom(instance, packet.Stream);
			}
			catch (Exception e)
			{
				// 出现任何异常都要断开Session，防止客户端伪造消息
				Log.Error(e);
				session.Error = ErrorCode.ERR_PacketParserError;
				session.Network.Remove(session.Id);
				return;
			}
			
			//Log.Debug($"recv: {JsonHelper.ToJson(message)}");
	
			switch (message)
			{
				case IFrameMessage iFrameMessage: // 如果是帧消息，构造成OneFrameMessage发给对应的unit
				{
					long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
					ActorMessageSender actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(unitId);
	
					// 这里设置了帧消息的id，防止客户端伪造
					iFrameMessage.Id = unitId;

					OneFrameMessage oneFrameMessage = new OneFrameMessage
					{
						Op = packet.Opcode,
						AMessage = ByteString.CopyFrom(session.Network.MessagePacker.SerializeTo(iFrameMessage))
					};
					actorMessageSender.Send(oneFrameMessage);
					return;
				}
				case IActorRequest iActorRequest: // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
				{
					long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
					ActorMessageSender actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(unitId);
	
					int rpcId = iActorRequest.RpcId; // 这里要保存客户端的rpcId
					IResponse response = await actorMessageSender.Call(iActorRequest);
					response.RpcId = rpcId;
	
					session.Reply(response);
					return;
				}
				case IActorMessage iActorMessage: // gate session收到actor消息直接转发给actor自己去处理
				{
					long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
					ActorMessageSender actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(unitId);
					actorMessageSender.Send(iActorMessage);
					return;
				}
			}
	
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(packet.Opcode, message));
		}
	}
}
