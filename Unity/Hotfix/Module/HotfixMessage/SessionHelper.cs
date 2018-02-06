using System;
using System.Threading.Tasks;
using Model;

namespace Hotfix
{
	public static class SessionHelper
	{
		public static void Send(this Session session, IMessage message)
		{
			ushort opcode = GetOpcode(message.GetType());
			byte[] bytes = ProtobufHelper.ToBytes(message);
			session.Send(opcode, bytes);
		}

		public static async Task<IResponse> Call(this Session session, IRequest request)
		{
			byte[] bytes = ProtobufHelper.ToBytes(request);
			ushort opcode = GetOpcode(request.GetType());
			PacketInfo packetInfo = await session.Call(opcode, bytes);
			ushort responseOpcode = packetInfo.Opcode;
			Type t = GetMessageType(responseOpcode);
			object aa = ProtobufHelper.FromBytes(t, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			IResponse response = (IResponse)aa;
			return response;
		}

		public static ushort GetOpcode(Type type)
		{
#if ILRuntime
			return Hotfix.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(type);
#else
			return Game.Scene.GetComponent<Model.OpcodeTypeComponent>().GetOpcode(type);
#endif
		}

		public static Type GetMessageType(ushort opcode)
		{
#if ILRuntime
			return Hotfix.Scene.GetComponent<OpcodeTypeComponent>().GetType(opcode);
#else
			return Game.Scene.GetComponent<Model.OpcodeTypeComponent>().GetType(opcode);
#endif
		}
	}
}
