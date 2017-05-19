using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
	public static class Opcode
	{
		public const ushort C2R_Login = 1;
		public const ushort R2C_Login = 2;
		public const ushort R2C_ServerLog = 3;
		public const ushort C2G_LoginGate = 4;
		public const ushort G2C_LoginGate = 5;
		public const ushort C2G_GetPlayerInfo = 6;
		public const ushort G2C_GetPlayerInfo = 7;
		public const ushort C2M_Reload = 8;
	}
}
