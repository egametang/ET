using System;
using System.Threading;
using System.Threading.Tasks;

namespace Model
{
	public static class SessionHelper
	{
		public static async Task<IResponse> Call(this Session session, IRequest request)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(request.GetType());
			byte[] bytes = session.network.MessagePacker.SerializeToByteArray(request);
			PacketInfo packetInfo = await session.Call(opcode, bytes);
			Type responseType = opcodeTypeComponent.GetType(packetInfo.Header.Opcode);
			object message = session.network.MessagePacker.DeserializeFrom(responseType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			return (IResponse) message;
		}

		public static async Task<IResponse> Call(this Session session, IRequest request, CancellationToken cancellationToken)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(request.GetType());
			byte[] bytes = session.network.MessagePacker.SerializeToByteArray(request);
			PacketInfo packetInfo = await session.Call(opcode, bytes, cancellationToken);
			Type responseType = opcodeTypeComponent.GetType(packetInfo.Header.Opcode);
			object message = session.network.MessagePacker.DeserializeFrom(responseType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			return (IResponse)message;
		}

		public static void Send(this Session session, IMessage message)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = session.network.MessagePacker.SerializeToByteArray(message);
			session.Send(opcode, bytes);
		}

		public static void Reply(this Session session, uint rpcId, IResponse message)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = session.network.MessagePacker.SerializeToByteArray(message);
			session.Reply(opcode, rpcId, bytes);
		}
	}
}
