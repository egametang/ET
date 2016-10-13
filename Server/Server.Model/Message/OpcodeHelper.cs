using System.Collections.Generic;

namespace Base
{
	public static class OpcodeHelper
	{
		private static readonly HashSet<Opcode> needDebugLogMessageSet = new HashSet<Opcode>
		{
			Opcode.S2C_StartGame,
			Opcode.S2C_LoginBattleServer,
			Opcode.S2C_StartPickHero,
			Opcode.S2C_PlayerSelectHero,
			
			Opcode.C2S_PlayerSelectHero,
			Opcode.C2S_LoginBattleServer,
			Opcode.C2S_StartOb,
			Opcode.S2C_StartOb,
			Opcode.S2C_LoadingFinishStartGame,
		};

		public static bool IsNeedDebugLogMessage(Opcode opcode)
		{
			//return true;
			if ((ushort)opcode > 1000)
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