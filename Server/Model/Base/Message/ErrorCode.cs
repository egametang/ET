namespace Model
{
	public static class ErrorCode
	{
		public const int ERR_Success = 0;

		public const int ERR_NotFoundActor = 1;

		public const int ERR_RpcFail = 101;
		public const int ERR_AccountOrPasswordError = 102;
		public const int ERR_ConnectGateKeyError = 103;
		public const int ERR_ReloadFail = 104;
		public const int ERR_NotFoundUnit = 105;
		public const int ERR_ActorLocationNotFound = 106;
	}
}