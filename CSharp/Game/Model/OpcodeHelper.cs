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
	}
}
