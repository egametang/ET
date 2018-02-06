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
			byte[] bytes = session.Network.MessagePacker.SerializeToByteArray(request);
			PacketInfo packetInfo = await session.Call(opcode, bytes);
			Type responseType = opcodeTypeComponent.GetType(packetInfo.Opcode);
			object message = session.Network.MessagePacker.DeserializeFrom(responseType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			IResponse response = (IResponse) message;
			if (response.Error > 100)
			{
				throw new RpcException(response.Error, response.Message);
			}

			return response;
		}

		public static async Task<IResponse> Call(this Session session, IRequest request, CancellationToken cancellationToken)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(request.GetType());
			byte[] bytes = session.Network.MessagePacker.SerializeToByteArray(request);
			PacketInfo packetInfo = await session.Call(opcode, bytes, cancellationToken);
			Type responseType = opcodeTypeComponent.GetType(packetInfo.Opcode);
			object message = session.Network.MessagePacker.DeserializeFrom(responseType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			IResponse response = (IResponse)message;
			if (response.Error > 100)
			{
				throw new RpcException(response.Error, response.Message);
			}

			return response;
		}

		public static void Send(this Session session, IMessage message)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = session.Network.MessagePacker.SerializeToByteArray(message);
			session.Send(opcode, bytes);
		}

		public static void Reply(this Session session, uint rpcId, IResponse message)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = session.Network.MessagePacker.SerializeToByteArray(message);
			session.Reply(opcode, rpcId, bytes);
		}
	}
}
