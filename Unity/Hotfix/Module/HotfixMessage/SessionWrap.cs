using System;
using System.Threading.Tasks;
using Model;

namespace Hotfix
{
	public class SessionWrap
	{
		public Session session;

		public void Send(IMessage message)
		{
#if !ILRuntime
			ushort opcode = Hotfix.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
			byte[] bytes = ProtobufHelper.ToBytes(message);
			this.session.Send(opcode, bytes);
#else
			this.session.Send(message);
#endif
		}

		public async Task<IResponse> Call(IRequest request)
		{
#if !ILRuntime
			byte[] bytes = ProtobufHelper.ToBytes(request);
			ushort opcode = Hotfix.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(request.GetType());
			PacketInfo packetInfo = await this.session.Call(opcode, bytes);
			ushort responseOpcode = BitConverter.ToUInt16(packetInfo.Bytes, 0);
			Type t = GetType(responseOpcode);
			IResponse response = (IResponse)ProtobufHelper.FromBytes(t, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
			return response;
#else
			AResponse response = await this.session.Call(request);
			return response;
#endif
		}

		public static Type GetType(ushort opcode)
		{
			return null;
		}
	}
}
