namespace ETModel
{
	public static class ErrorCode
	{
		public const int ERR_Success = 0;
		
		// 100000 以上，避免跟SocketError冲突
		public const int ERR_MyErrorCode = 100000;
		

		
		public const int ERR_Exception = 200000;
		
		public const int ERR_NotFoundActor = 100002;
		public const int ERR_ActorNoMailBoxComponent = 100003;
		public const int ERR_ActorTimeOut = 100004;
		public const int ERR_PacketParserError = 100005;

		public const int ERR_AccountOrPasswordError = 100102;
		public const int ERR_SessionActorError = 100103;
		public const int ERR_NotFoundUnit = 100104;
		public const int ERR_ConnectGateKeyError = 100105;

		public const int ERR_RpcFail = 102001;
		public const int ERR_SocketDisconnected = 102002;
		public const int ERR_ReloadFail = 102003;
		public const int ERR_ActorLocationNotFound = 102004;

		public static bool IsRpcNeedThrowException(int error)
		{
			if (error == 0)
			{
				return false;
			}

			if (error > ERR_Exception)
			{
				return false;
			}

			return true;
		}
	}
}