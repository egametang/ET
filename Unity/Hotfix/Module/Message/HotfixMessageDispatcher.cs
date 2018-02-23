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
			object message = ProtobufHelper.FromBytes(t, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			Hotfix.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(packetInfo.RpcId, packetInfo.Opcode, message));
		}
	}
}
