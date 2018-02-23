namespace Model
{
	public static partial class Opcode
	{
	    public const ushort M2M_TrasferUnitRequest = 2005;
	    public const ushort M2M_TrasferUnitResponse = 2006;
	    public const ushort M2A_Reload = 2007;
	    public const ushort A2M_Reload = 2008;
	    public const ushort G2G_LockRequest = 2009;
	    public const ushort G2G_LockResponse = 2010;
	    public const ushort G2G_LockReleaseRequest = 2011;
	    public const ushort G2G_LockReleaseResponse = 2012;
	    public const ushort DBSaveRequest = 2013;
	    public const ushort DBSaveBatchResponse = 2014;
	    public const ushort DBSaveBatchRequest = 2015;
	    public const ushort DBSaveResponse = 2016;
	    public const ushort DBQueryRequest = 2017;
	    public const ushort DBQueryResponse = 2018;
	    public const ushort DBQueryBatchRequest = 2019;
	    public const ushort DBQueryBatchResponse = 2020;
	    public const ushort DBQueryJsonRequest = 2021;
	    public const ushort DBQueryJsonResponse = 2022;
	    public const ushort ObjectAddRequest = 2023;
	    public const ushort ObjectAddResponse = 2024;
	    public const ushort ObjectRemoveRequest = 2025;
	    public const ushort ObjectRemoveResponse = 2026;
	    public const ushort ObjectLockRequest = 2027;
	    public const ushort ObjectLockResponse = 2028;
	    public const ushort ObjectUnLockRequest = 2029;
	    public const ushort ObjectUnLockResponse = 2030;
	    public const ushort ObjectGetRequest = 2031;
	    public const ushort ObjectGetResponse = 2032;
	    public const ushort R2G_GetLoginKey = 2033;
	    public const ushort G2R_GetLoginKey = 2034;
	    public const ushort G2M_CreateUnit = 2035;
	    public const ushort M2G_CreateUnit = 2036;
	}
}
