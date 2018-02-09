using System;
using Model;

namespace Hotfix
{
	public static class HotfixMessageDispatcher
	{
		public static void Run(Session session, PacketInfo packetInfo)
		{
			ushort opcode = packetInfo.Opcode;
			Type t = Hotfix.Scene.GetComponent<OpcodeTypeComponent>().GetType(opcode);
			object aa = ProtobufHelper.FromBytes(t, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			IMessage message = (IMessage)aa;
			Hotfix.Scene.GetComponent<MessageDispatherComponent>().Handle(session, opcode, message);
		}
	}
}
