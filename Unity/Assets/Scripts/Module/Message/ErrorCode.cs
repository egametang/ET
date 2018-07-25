namespace ETModel
{
	public static class ErrorCode
	{
		public const int ERR_Success = 0;
		
		// 100000 以上，避免跟SocketError冲突
		public const int ERR_MyErrorCode = 100000;
		

		// 小于这个Rpc会抛异常
		public const int ERR_Exception = 200000;
		
		public const int ERR_NotFoundActor = 200002;
		public const int ERR_ActorNoMailBoxComponent = 200003;
		public const int ERR_ActorTimeOut = 200004;
		public const int ERR_PacketParserError = 200005;

		public const int ERR_AccountOrPasswordError = 200102;
		public const int ERR_SessionActorError = 200103;
		public const int ERR_NotFoundUnit = 200104;
		public const int ERR_ConnectGateKeyError = 200105;

		public const int ERR_RpcFail = 202001;
		public const int ERR_SocketDisconnected = 202002;
		public const int ERR_ReloadFail = 202003;
		public const int ERR_ActorLocationNotFound = 202004;
		public const int ERR_KcpConnectFail = 202005;
		public const int ERR_KcpTimeout = 202006;
		public const int ERR_KcpRemoteDisconnect = 202007;

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