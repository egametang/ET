using System;

namespace Model
{
	public class MessageInfo
	{
		public byte[] MessageBytes;
		public int Offset;
		public int Count;
		public uint RpcId;
	}

	public interface IMHandler
	{
		void Handle(Session session, ushort opcode, MessageInfo messageInfo);
		Type GetMessageType();
	}
}
