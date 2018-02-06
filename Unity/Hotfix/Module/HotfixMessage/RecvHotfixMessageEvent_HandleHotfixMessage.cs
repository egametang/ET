using System;
using Model;

namespace Hotfix
{
	// 分发数值监听
	[Event((int)Model.EventIdType.RecvHotfixMessage)]
	public class RecvHotfixMessageEvent_HandleHotfixMessage: IEvent<Session, PacketInfo>
	{
		public void Run(Session session, PacketInfo packetInfo)
		{
			ushort opcode = packetInfo.Opcode;
			Type t = SessionHelper.GetMessageType(opcode);
			object aa = ProtobufHelper.FromBytes(t, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			IMessage message = (IMessage)aa;
			Hotfix.Scene.GetComponent<MessageDispatherComponent>().Handle(session, opcode, message);
		}
	}
}
