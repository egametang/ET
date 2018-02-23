using System;

namespace Model
{
	public class ClientDispatcher: IMessageDispatcher
	{
		// 热更层消息回调
		public Action<Session, PacketInfo> HotfixCallback;

		public void Dispatch(Session session, PacketInfo packetInfo)
		{
			if (OpcodeHelper.IsClientHotfixMessage(packetInfo.Opcode))
			{
				HotfixCallback.Invoke(session, packetInfo);
				return;
			}

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
			MessageInfo messageInfo = new MessageInfo(packetInfo.RpcId, packetInfo.Opcode, message);
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, messageInfo);
		}
	}
}
