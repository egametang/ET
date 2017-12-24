namespace Model
{
	public static class ErrorCode
	{
		public const int ERR_Success = 0;

		public const int ERR_NotFoundActor = 1;

        /// <summary>
        /// 查询用户信息错误
        /// </summary>
	    public const int ErrQueryUserInfoError = 12;
	    public const int ErrStartMatchError = 14;
	    public const int ErrUserMoneyLessError = 15;

        public const int ERR_RpcFail = 101;
		public const int ERR_AccountOrPasswordError = 102;
		public const int ERR_ConnectGateKeyError = 103;
		public const int ERR_ReloadFail = 104;
		public const int ERR_NotFoundUnit = 105;
		public const int ERR_ActorLocationNotFound = 106;
		public const int ERR_SessionActorError = 107;
		public const int ERR_ActorError = 108;
	}
}