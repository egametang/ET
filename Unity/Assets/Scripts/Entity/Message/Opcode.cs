namespace Model
{
	// 1000开始
	public enum Opcode: ushort
	{
		FrameMessage = 1000,
		C2R_Login = 1001,
		R2C_Login = 1002,
		R2C_ServerLog = 1003,
		C2G_LoginGate = 1004,
		G2C_LoginGate = 1005,
		C2G_EnterMap = 1006,
		G2C_EnterMap = 1007,
		C2M_Reload = 1008,
		M2C_Reload = 1009,
		C2R_Ping = 1010,
		R2C_Ping = 1011,

		Actor_Test = 2001,
		Actor_TestRequest = 2002,
		Actor_TestResponse = 2003,
		Actor_TransferRequest = 2004,
		Actor_TransferResponse = 2005,
		Frame_ClickMap = 2006,
		Actor_CreateUnits = 2007,


		// server inner opcode
		ActorRequest = 1,
		ActorResponse = 2,
		ActorRpcRequest = 3,
		ActorRpcResponse = 4,
		G2G_LockRequest = 10,
		G2G_LockResponse = 11,
		G2G_LockReleaseRequest = 12,
		G2G_LockReleaseResponse = 13,

		M2A_Reload = 20,
		A2M_Reload = 21,

		DBSaveRequest = 26,
		DBSaveResponse = 27,
		DBQueryRequest = 28,
		DBQueryResponse = 29,
		DBSaveBatchResponse = 37,
		DBSaveBatchRequest = 38,
		DBQueryBatchRequest = 61,
		DBQueryBatchResponse = 62,
		DBQueryJsonRequest = 65,
		DBQueryJsonResponse = 66,

		ObjectAddRequest = 70,
		ObjectAddResponse = 71,
		ObjectRemoveRequest = 72,
		ObjectRemoveResponse = 73,
		ObjectLockRequest = 74,
		ObjectLockResponse = 75,
		ObjectUnLockRequest = 76,
		ObjectUnLockResponse = 77,
		ObjectGetRequest = 78,
		ObjectGetResponse = 79,

		R2G_GetLoginKey = 101,
		G2R_GetLoginKey = 102,

		G2M_CreateUnit = 103,
		M2G_CreateUnit = 104,

		M2M_TrasferUnitRequest = 105,
		M2M_TrasferUnitResponse = 106,
	}
}
