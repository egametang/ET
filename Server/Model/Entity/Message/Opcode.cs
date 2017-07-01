namespace Model
{
	public static class Opcode
	{
		public const ushort G2G_LockRequest = 10;
		public const ushort G2G_LockResponse = 11;
		public const ushort G2G_LockReleaseRequest = 12;
		public const ushort G2G_LockReleaseResponse = 13;

		public const ushort M2A_Reload = 20;
		public const ushort A2M_Reload = 21;

		public const ushort DBSaveRequest = 26;
		public const ushort DBSaveResponse = 27;
		public const ushort DBQueryRequest = 28;
		public const ushort DBQueryResponse = 29;
		public const ushort DBSaveBatchResponse = 37;
		public const ushort DBSaveBatchRequest = 38;
		public const ushort DBQueryBatchRequest = 61;
		public const ushort DBQueryBatchResponse = 62;
		public const ushort DBQueryJsonRequest = 65;
		public const ushort DBQueryJsonResponse = 66;

		public const ushort C2R_Login = 1000;
		public const ushort R2C_Login = 1002;
		public const ushort R2C_ServerLog = 1003;
		public const ushort C2G_LoginGate = 1004;
		public const ushort G2C_LoginGate = 1005;
		public const ushort C2G_GetPlayerInfo = 1006;
		public const ushort G2C_GetPlayerInfo = 1007;
		public const ushort C2M_Reload = 1008;

		public const ushort R2G_GetLoginKey = 10001;
		public const ushort G2R_GetLoginKey = 10002;
	}
}
