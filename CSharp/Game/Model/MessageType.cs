namespace Model
{
	public static class MessageType
	{
	#region client message 0
		public const int CMsgLogin = 1;
	#endregion client message 10000

	#region server message 10000
	#endregion server message 20000

	#region rpc request message 20000
	#endregion rpc request message 30000

	#region rpc request message 30000
	#endregion rpc request message 40000
	}

	public static class MessageTypeHelper
	{
		public static bool IsClientMessage(int opcode)
		{
			if (opcode > 0 && opcode < 10000)
			{
				return true;
			}
			return false;
		}

		public static bool IsServerMessage(int opcode)
		{
			if (opcode > 10000 && opcode < 20000)
			{
				return true;
			}
			return false;
		}

		public static bool IsRpcRequestMessage(int opcode)
		{
			if (opcode > 20000 && opcode < 30000)
			{
				return true;
			}
			return false;
		}

		public static bool IsRpcResponseMessage(int opcode)
		{
			if (opcode > 30000 && opcode < 40000)
			{
				return true;
			}
			return false;
		}
	}
}