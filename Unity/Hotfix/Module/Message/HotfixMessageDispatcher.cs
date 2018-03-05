using System;
using ETModel;

namespace ETHotfix
{
	public static class HotfixMessageDispatcher
	{
		public static void Run(Session session, PacketInfo packetInfo)
		{
			ushort opcode = packetInfo.Opcode;
			Type t = Game.Scene.GetComponent<OpcodeTypeComponent>().GetType(opcode);
			object message = ProtobufHelper.FromBytes(t, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(packetInfo.RpcId, packetInfo.Opcode, message));
		}
	}
}
