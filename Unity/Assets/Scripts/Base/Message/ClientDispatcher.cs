using System;

namespace Model
{
	public class ClientDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, PacketInfo packetInfo)
		{
			Type messageType = Game.Scene.GetComponent<OpcodeTypeComponent>().GetType(packetInfo.Header.Opcode);
			IMessage message = (IMessage)session.network.MessagePacker.DeserializeFrom(messageType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);

			// 如果是帧同步消息,交给ClientFrameComponent处理
			FrameMessage frameMessage = message as FrameMessage;
			if (frameMessage != null)
			{
				Game.Scene.GetComponent<ClientFrameComponent>().Add(session, frameMessage);
				return;
			}

			// 普通消息或者是Rpc请求消息
			if (message is IMessage || message is IRequest)
			{
				MessageInfo messageInfo = new MessageInfo(packetInfo.Header.Opcode, message);
				Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, messageInfo);
				return;
			}

			throw new Exception($"message type error: {message.GetType().FullName}");
		}
	}
}
