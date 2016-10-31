using System;

namespace Model
{
	public class MessageInfo
	{
		public ushort Opcode { get; }
		public byte[] MessageBytes { get; }
		public int Offset { get; }
		public uint RpcId { get; }

		public MessageInfo(ushort opcode, byte[] messageBytes, int offset, uint rpcId)
		{
			this.Opcode = opcode;
			this.MessageBytes = messageBytes;
			this.Offset = offset;
			this.RpcId = rpcId;
		}

		public int Count
		{
			get
			{
				return this.MessageBytes.Length - this.Offset;
			}
		}
	}

	public interface IMHandler
	{
		void Handle(Session session, MessageInfo messageInfo);
		Type GetMessageType();
	}
}
