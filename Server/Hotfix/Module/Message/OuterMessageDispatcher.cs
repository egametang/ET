using System;
using ETModel;

namespace ETHotfix
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public async void Dispatch(Session session, Packet packet)
		{
			ushort opcode = packet.Opcode();
			Type messageType = Game.Scene.GetComponent<OpcodeTypeComponent>().GetType(opcode);
			object message = session.Network.MessagePacker.DeserializeFrom(messageType, packet.Bytes, Packet.Index, packet.Length - Packet.Index);
			
			// 如果是帧指令消息，构造成OneFrameMessage发给对应的unit
			if (message is IFrameMessage)
			{
				long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
				ActorProxy actorProxy = Game.Scene.GetComponent<ActorProxyComponent>().Get(unitId);

				// 这里设置了帧消息的id，防止客户端伪造
				IFrameMessage iFrameMessage = (IFrameMessage)message;
				iFrameMessage.Id = unitId;

				OneFrameMessage oneFrameMessage = new OneFrameMessage();
				oneFrameMessage.Op = opcode;
				oneFrameMessage.AMessage = session.Network.MessagePacker.SerializeToByteArray(iFrameMessage);
				actorProxy.Send(oneFrameMessage);
				return;
			}

			// gate session收到actor消息直接转发给actor自己去处理
			if (message is IActorMessage)
			{
				long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
				ActorProxy actorProxy = Game.Scene.GetComponent<ActorProxyComponent>().Get(unitId);
				actorProxy.Send((IMessage)message);
				return;
			}

			// gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端
			if (message is IActorRequest aActorRequest)
			{
				long unitId = session.GetComponent<SessionPlayerComponent>().Player.UnitId;
				ActorProxy actorProxy = Game.Scene.GetComponent<ActorProxyComponent>().Get(unitId);
				IResponse response = await actorProxy.Call(aActorRequest);
				session.Reply(response);
				return;
			}

			if (message != null)
			{
				Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(opcode, message));
				return;
			}

			throw new Exception($"message type error: {message.GetType().FullName}");
		}

		public void Dispatch(Session session, ushort opcode, object message)
		{
			throw new NotImplementedException();
		}
	}
}
