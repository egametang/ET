using System;

namespace ETModel
{
	public class ClientDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, Packet packet)
		{
			ushort opcode = packet.Opcode();
			
			if (OpcodeHelper.IsClientHotfixMessage(opcode))
			{
				session.GetComponent<SessionCallbackComponent>().MessageCallback.Invoke(session, packet);
				return;
			}

			OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
			Type responseType = opcodeTypeComponent.GetType(opcode);
			object message = session.Network.MessagePacker.DeserializeFrom(responseType, packet.Bytes, Packet.Index, packet.Length - Packet.Index);
			// 如果是帧同步消息,交给ClientFrameComponent处理
			FrameMessage frameMessage = message as FrameMessage;
			if (frameMessage != null)
			{
				Game.Scene.GetComponent<ClientFrameComponent>().Add(session, frameMessage);
				return;
			}

			// 普通消息或者是Rpc请求消息
			MessageInfo messageInfo = new MessageInfo(opcode, message);
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, messageInfo);
		}
	}
}
