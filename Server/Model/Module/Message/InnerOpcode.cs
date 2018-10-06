namespace ETModel
{
	public static partial class InnerOpcode
	{
		 public const ushort M2M_TrasferUnitRequest = 1001;
		 public const ushort M2M_TrasferUnitResponse = 1002;
		 public const ushort M2A_Reload = 1003;
		 public const ushort A2M_Reload = 1004;
		 public const ushort G2G_LockRequest = 1005;
		 public const ushort G2G_LockResponse = 1006;
		 public const ushort G2G_LockReleaseRequest = 1007;
		 public const ushort G2G_LockReleaseResponse = 1008;
		 public const ushort DBSaveRequest = 1009;
		 public const ushort DBSaveBatchResponse = 1010;
		 public const ushort DBSaveBatchRequest = 1011;
		 public const ushort DBSaveResponse = 1012;
		 public const ushort DBQueryRequest = 1013;
		 public const ushort DBQueryResponse = 1014;
		 public const ushort DBQueryBatchRequest = 1015;
		 public const ushort DBQueryBatchResponse = 1016;
		 public const ushort DBQueryJsonRequest = 1017;
		 public const ushort DBQueryJsonResponse = 1018;
		 public const ushort ObjectAddRequest = 1019;
		 public const ushort ObjectAddResponse = 1020;
		 public const ushort ObjectRemoveRequest = 1021;
		 public const ushort ObjectRemoveResponse = 1022;
		 public const ushort ObjectLockRequest = 1023;
		 public const ushort ObjectLockResponse = 1024;
		 public const ushort ObjectUnLockRequest = 1025;
		 public const ushort ObjectUnLockResponse = 1026;
		 public const ushort ObjectGetRequest = 1027;
		 public const ushort ObjectGetResponse = 1028;
		 public const ushort R2G_GetLoginKey = 1029;
		 public const ushort G2R_GetLoginKey = 1030;
		 public const ushort G2M_CreateUnit = 1031;
		 public const ushort M2G_CreateUnit = 1032;
		 public const ushort G2M_SessionDisconnect = 1033;
	}
}
