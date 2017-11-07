namespace Model
{
	// 客户端 100 - 999, 服务端内部1000以上
	public enum Opcode: ushort
	{
		ARequest = 1,
		AResponse = 2,
		AActorMessage = 3,
		AActorRequest = 4,
		AActorResponse = 5,
		ActorRequest = 6,
		ActorResponse = 7,
		ActorRpcRequest = 8,
		ActorRpcResponse = 9,
		AFrameMessage = 10,

		FrameMessage = 100,
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

		// 服务端Opcode, 从1000开始
		G2G_LockRequest = 1000,
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
	}
}
