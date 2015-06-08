namespace Model
{
	public static class OpcodeHelper
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
