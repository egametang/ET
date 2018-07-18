using System;

namespace ETModel
{
	public class ClientDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, Packet packet)
		{
			object message;
			try
			{
				if (OpcodeHelper.IsClientHotfixMessage(packet.Opcode))
				{
					session.GetComponent<SessionCallbackComponent>().MessageCallback.Invoke(session, packet);
					return;
				}

				OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
				object instance = opcodeTypeComponent.GetInstance(packet.Opcode);
				message = session.Network.MessagePacker.DeserializeFrom(instance, packet.Stream);
			}
			catch (Exception e)
			{
				// 出现任何解析消息异常都要断开Session，防止客户端伪造消息
				Log.Error(e);
				session.Error = ErrorCode.ERR_PacketParserError;
				session.Network.Remove(session.Id);
				return;
			}
				
			// 如果是帧同步消息,交给ClientFrameComponent处理
			FrameMessage frameMessage = message as FrameMessage;
			if (frameMessage != null)
			{
				Game.Scene.GetComponent<ClientFrameComponent>().Add(session, frameMessage);
				return;
			}

			// 普通消息或者是Rpc请求消息
			MessageInfo messageInfo = new MessageInfo(packet.Opcode, message);
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, messageInfo);
		}
	}
}
