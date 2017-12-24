namespace Model
{
	public enum Opcode: ushort
	{
        #region 客户端 1 +

	    // 客户端开始编号从 1 开始递增
        ClinetNoStart = 1,

        #region Demo自带

        ARequest,
        AResponse,
        AActorMessage,
        AActorRequest,
        AActorResponse,
        ActorRequest,
        ActorResponse,
        ActorRpcRequest,
        ActorRpcResponse,
        AFrameMessage,
        FrameMessage,
        C2R_Login,
        R2C_Login,
        R2C_ServerLog,
        C2G_LoginGate,
        G2C_LoginGate,
        C2G_EnterMap,
        G2C_EnterMap,
        C2M_Reload,
        M2C_Reload,
        C2R_Ping,
        R2C_Ping,

        Actor_Test,
        Actor_TestRequest,
        Actor_TestResponse,
        Actor_TransferRequest,
        Actor_TransferResponse,
        Frame_ClickMap,
        Actor_CreateUnits,


        #endregion

        /// <summary>
        /// 退出
        /// </summary>
	    Quit,

        /// <summary>
        /// 请求适配
        /// </summary>
	    StartMatchRt,

        /// <summary>
        /// 适配结果
        /// </summary>
	    StartMatchRe,

        /// <summary>
        /// 修改游戏模式
        /// </summary>
	    ChangeGameMode,

        #endregion

        #region 服务端 10000 +

        // 服务端开始编号从 10000 开始递增
        ServerNoStart = 10000,

        #region Gate

        /// <summary>
        /// 获取授权Key
        /// </summary>
	    GetLoginKeyRt,

        /// <summary>
        /// 返回授权Key
        /// </summary>
	    GetLoginKeyRe,

        /// <summary>
        /// 用户信息 请求
        /// </summary>
	    GetUserInfoRt,

        /// <summary>
        /// 用户信息 应答
        /// </summary>
	    GetUserInfoRe,

        /// <summary>
        /// 登录授权 请求
        /// </summary>
	    LoginGateRt,

        /// <summary>
        /// 登录授权 应答
        /// </summary>
	    LoginGateRe,
        
        /// <summary>
        /// 加入适配请求
        /// </summary>
        JoinMatchRt,

        /// <summary>
        /// 加入适配应答
        /// </summary>
        JoinMatchRe,


        #endregion

        #region 网关

        /// <summary>
        /// 用户断开
        /// </summary>
        PlayerDisconnect,

        /// <summary>
        /// RoomKey
        /// </summary>
	    RoomKey,

        /// <summary>
        /// 用户退出
        /// </summary>
	    PlayerQuit,

        #endregion

        #region Match

        /// <summary>
        /// 匹配成功
        /// </summary>
	    MatchSuccess,

        #endregion

        #region ddz

        /// <summary>
        /// 玩家退出房间
        /// </summary>
        GamerQuitRoom,

        /// <summary>
        /// 创建房间 请求
        /// </summary>
	    CreateRoomRt,

        /// <summary>
        /// 创建房间 结果
        /// </summary>
	    CreateRoomRe,

        /// <summary>
        /// 玩家弃牌
        /// </summary>
	    Discard,

        #endregion

        #region Demo自带

        G2G_LockRequest,
        G2G_LockResponse,
        G2G_LockReleaseRequest,
        G2G_LockReleaseResponse,

        M2A_Reload,
        A2M_Reload,

        DBSaveRequest,
        DBSaveResponse,
        DBQueryRequest,
        DBQueryResponse,
        DBSaveBatchResponse,
        DBSaveBatchRequest,
        DBQueryBatchRequest,
        DBQueryBatchResponse,
        DBQueryJsonRequest,
        DBQueryJsonResponse,

        ObjectAddRequest,
        ObjectAddResponse,
        ObjectRemoveRequest,
        ObjectRemoveResponse,
        ObjectLockRequest,
        ObjectLockResponse,
        ObjectUnLockRequest,
        ObjectUnLockResponse,
        ObjectGetRequest,
        ObjectGetResponse,

        R2G_GetLoginKey,
        G2R_GetLoginKey,

        G2M_CreateUnit,
        M2G_CreateUnit,

        M2M_TrasferUnitRequest,
        M2M_TrasferUnitResponse,

        #endregion


        #endregion

    }
}
