using System.Collections.Generic;

namespace Model
{
	public static class OpcodeHelper
	{
		private static readonly HashSet<ushort> needDebugLogMessageSet = new HashSet<ushort> { 1 };

		public static bool IsNeedDebugLogMessage(ushort opcode)
		{
			//return true;
			if (opcode > 1000)
			{
				return true;
			}

			if (needDebugLogMessageSet.Contains(opcode))
			{
				return true;
			}

			return false;
		}
	}
}