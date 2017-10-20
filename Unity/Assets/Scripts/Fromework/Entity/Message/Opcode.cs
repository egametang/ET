namespace Model
{
	// 1000开始
	public static partial class Opcode
	{
		public const ushort FrameMessage = 1000;
		public const ushort C2R_Login = 1001;
		public const ushort R2C_Login = 1002;
		public const ushort R2C_ServerLog = 1003;
		public const ushort C2G_LoginGate = 1004;
		public const ushort G2C_LoginGate = 1005;
		public const ushort C2G_EnterMap = 1006;
		public const ushort G2C_EnterMap = 1007;
		public const ushort C2M_Reload = 1008;

		public const ushort Actor_Test = 2001;
		public const ushort Actor_TestRequest = 2002;
		public const ushort Actor_TestResponse = 2003;
		public const ushort Actor_TransferRequest = 2004;
		public const ushort Actor_TransferResponse = 2005;
		public const ushort Frame_ClickMap = 2006;
		public const ushort Actor_CreateUnits = 2007;
	}
}
