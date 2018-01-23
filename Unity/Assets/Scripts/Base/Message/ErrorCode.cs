namespace Model
{
	public static class ErrorCode
	{
		// 框架内部错误
		public const int ERR_Success = 0;
		public const int ERR_NotFoundActor = 2;

		public const int ERR_RpcFail = 101;
		public const int ERR_SocketDisconnected = 102;
		public const int ERR_ReloadFail = 103;
		public const int ERR_ActorLocationNotFound = 104;

		// 逻辑相关错误
		public const int ERR_AccountOrPasswordError = 202;
		public const int ERR_ConnectGateKeyError = 203;
		public const int ERR_NotFoundUnit = 204;
		public const int ERR_SessionActorError = 207;
		
	}
}