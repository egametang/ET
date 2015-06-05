namespace Model
{
	public enum Opcode: short
	{
		CMsgLogin = 1,
		RpcResponse = 30000,
		RpcException = 30001,
	}

	public static class MessageTypeHelper
	{
		public static bool IsClientMessage(Opcode opcode)
		{
			if ((ushort)opcode > 0 && (ushort)opcode < 10000)
			{
				return true;
			}
			return false;
		}

		public static bool IsServerMessage(Opcode opcode)
		{
			if ((ushort)opcode > 10000 && (ushort)opcode < 20000)
			{
				return true;
			}
			return false;
		}

		public static bool IsRpcRequestMessage(Opcode opcode)
		{
			if ((ushort)opcode > 20000 && (ushort)opcode < 30000)
			{
				return true;
			}
			return false;
		}

		public static bool IsRpcResponseMessage(Opcode opcode)
		{
			if ((ushort)opcode > 30000 && (ushort)opcode < 40000)
			{
				return true;
			}
			return false;
		}
	}
}