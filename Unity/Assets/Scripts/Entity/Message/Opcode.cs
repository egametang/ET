namespace Model
{
	public static partial class Opcode
	{
		 public const ushort C2R_Login = 101;
		 public const ushort R2C_Login = 102;
		 public const ushort C2G_LoginGate = 103;
		 public const ushort G2C_LoginGate = 104;
		 public const ushort Actor_Test = 105;
		 public const ushort Actor_TestRequest = 106;
		 public const ushort Actor_TestResponse = 107;
		 public const ushort Actor_TransferRequest = 108;
		 public const ushort Actor_TransferResponse = 109;
		 public const ushort C2G_EnterMap = 110;
		 public const ushort G2C_EnterMap = 111;
		 public const ushort UnitInfo = 112;
		 public const ushort Actor_CreateUnits = 113;
		 public const ushort FrameMessageInfo = 114;
		 public const ushort FrameMessage = 115;
		 public const ushort Frame_ClickMap = 116;
		 public const ushort C2M_Reload = 117;
		 public const ushort M2C_Reload = 118;
		 public const ushort C2R_Ping = 119;
		 public const ushort R2C_Ping = 120;
		 public const ushort ActorRequest = 1001;
		 public const ushort ActorResponse = 1002;
		 public const ushort ActorRpcRequest = 1003;
		 public const ushort ActorRpcResponse = 1004;
		 public const ushort M2M_TrasferUnitRequest = 1005;
		 public const ushort M2M_TrasferUnitResponse = 1006;
		 public const ushort M2A_Reload = 1007;
		 public const ushort A2M_Reload = 1008;
		 public const ushort G2G_LockRequest = 1009;
		 public const ushort G2G_LockResponse = 1010;
		 public const ushort G2G_LockReleaseRequest = 1011;
		 public const ushort G2G_LockReleaseResponse = 1012;
		 public const ushort DBSaveRequest = 1013;
		 public const ushort DBSaveBatchResponse = 1014;
		 public const ushort DBSaveBatchRequest = 1015;
		 public const ushort DBSaveResponse = 1016;
		 public const ushort DBQueryRequest = 1017;
		 public const ushort DBQueryResponse = 1018;
		 public const ushort DBQueryBatchRequest = 1019;
		 public const ushort DBQueryBatchResponse = 1020;
		 public const ushort DBQueryJsonRequest = 1021;
		 public const ushort DBQueryJsonResponse = 1022;
		 public const ushort ObjectAddRequest = 1023;
		 public const ushort ObjectAddResponse = 1024;
		 public const ushort ObjectRemoveRequest = 1025;
		 public const ushort ObjectRemoveResponse = 1026;
		 public const ushort ObjectLockRequest = 1027;
		 public const ushort ObjectLockResponse = 1028;
		 public const ushort ObjectUnLockRequest = 1029;
		 public const ushort ObjectUnLockResponse = 1030;
		 public const ushort ObjectGetRequest = 1031;
		 public const ushort ObjectGetResponse = 1032;
		 public const ushort R2G_GetLoginKey = 1033;
		 public const ushort G2R_GetLoginKey = 1034;
		 public const ushort G2M_CreateUnit = 1035;
		 public const ushort M2G_CreateUnit = 1036;
	}
}
