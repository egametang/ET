using System;

namespace Model
{
	public class ClientDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, PacketInfo packetInfo)
		{
#if ILRuntime
// 热更消息抛到hotfix层
			if (OpcodeHelper.IsClientHotfixMessage(packetInfo.Header.Opcode))
			{
				Game.EventSystem.Run(EventIdType.RecvHotfixMessage, packetInfo);
				return;
			}
#endif


			Type messageType = Game.Scene.GetComponent<OpcodeTypeComponent>().GetType(packetInfo.Opcode);
			IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(messageType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);

			// 如果是帧同步消息,交给ClientFrameComponent处理
			FrameMessage frameMessage = message as FrameMessage;
			if (frameMessage != null)
			{
				Game.Scene.GetComponent<ClientFrameComponent>().Add(session, frameMessage);
				return;
			}

			// 普通消息或者是Rpc请求消息
			MessageInfo messageInfo = new MessageInfo(packetInfo.Opcode, message);
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, messageInfo);
		}
	}
}
