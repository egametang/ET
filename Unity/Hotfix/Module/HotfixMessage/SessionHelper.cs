using System;
using System.Threading.Tasks;
using Model;

namespace Hotfix
{
	public static class SessionHelper
	{
		public static void Send(this Session session, IMessage message)
		{
			ushort opcode = Hotfix.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
			byte[] bytes = ProtobufHelper.ToBytes(message);
			session.Send(opcode, bytes);
		}

		public static async Task<IResponse> Call(this Session session, IRequest request)
		{
			OpcodeTypeComponent opcodeTypeComponent = Hotfix.Scene.GetComponent<OpcodeTypeComponent>();
			byte[] bytes = ProtobufHelper.ToBytes(request);
			ushort opcode = opcodeTypeComponent.GetOpcode(request.GetType());
			PacketInfo packetInfo = await session.Call(opcode, bytes);
			ushort responseOpcode = packetInfo.Opcode;
			Type t = opcodeTypeComponent.GetType(responseOpcode);
			object aa = ProtobufHelper.FromBytes(t, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			IResponse response = (IResponse)aa;
			return response;
		}
	}
}
