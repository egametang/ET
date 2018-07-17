using ETModel;
using System.Collections.Generic;
namespace ETHotfix
{
/// <summary>
/// 传送unit
/// </summary>
	[Message(InnerOpcode.M2M_TrasferUnitRequest)]
	public partial class M2M_TrasferUnitRequest: IRequest
	{
		public int RpcId { get; set; }
		public Unit Unit;

	}

	[Message(InnerOpcode.M2M_TrasferUnitResponse)]
	public partial class M2M_TrasferUnitResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long InstanceId;

	}

	[Message(InnerOpcode.M2A_Reload)]
	public partial class M2A_Reload: IRequest
	{
		public int RpcId { get; set; }

	}

	[Message(InnerOpcode.A2M_Reload)]
	public partial class A2M_Reload: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.G2G_LockRequest)]
	public partial class G2G_LockRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id;

		public string Address;

	}

	[Message(InnerOpcode.G2G_LockResponse)]
	public partial class G2G_LockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.G2G_LockReleaseRequest)]
	public partial class G2G_LockReleaseRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id;

		public string Address;

	}

	[Message(InnerOpcode.G2G_LockReleaseResponse)]
	public partial class G2G_LockReleaseResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBSaveRequest)]
	public partial class DBSaveRequest: IRequest
	{
		public int RpcId { get; set; }

		public bool NeedCache;

		public string CollectionName;

		public ComponentWithId Component;

	}

	[Message(InnerOpcode.DBSaveBatchResponse)]
	public partial class DBSaveBatchResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBSaveBatchRequest)]
	public partial class DBSaveBatchRequest: IRequest
	{
		public int RpcId { get; set; }

		public bool NeedCache;

		public string CollectionName;

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.DBSaveResponse)]
	public partial class DBSaveResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBQueryRequest)]
	public partial class DBQueryRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Id;

		public string CollectionName;

		public bool NeedCache;

	}

	[Message(InnerOpcode.DBQueryResponse)]
	public partial class DBQueryResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public ComponentWithId Component;

	}

	[Message(InnerOpcode.DBQueryBatchRequest)]
	public partial class DBQueryBatchRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName;

		public List<long> IdList = new List<long>();

		public bool NeedCache;

	}

	[Message(InnerOpcode.DBQueryBatchResponse)]
	public partial class DBQueryBatchResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.DBQueryJsonRequest)]
	public partial class DBQueryJsonRequest: IRequest
	{
		public int RpcId { get; set; }

		public string CollectionName;

		public string Json;

	}

	[Message(InnerOpcode.DBQueryJsonResponse)]
	public partial class DBQueryJsonResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.ObjectAddRequest)]
	public partial class ObjectAddRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key;

		public long InstanceId;

	}

	[Message(InnerOpcode.ObjectAddResponse)]
	public partial class ObjectAddResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectRemoveRequest)]
	public partial class ObjectRemoveRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key;

	}

	[Message(InnerOpcode.ObjectRemoveResponse)]
	public partial class ObjectRemoveResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectLockRequest)]
	public partial class ObjectLockRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key;

		public long InstanceId;

		public int Time;

	}

	[Message(InnerOpcode.ObjectLockResponse)]
	public partial class ObjectLockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectUnLockRequest)]
	public partial class ObjectUnLockRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key;

		public long OldInstanceId;

		public long InstanceId;

	}

	[Message(InnerOpcode.ObjectUnLockResponse)]
	public partial class ObjectUnLockResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectGetRequest)]
	public partial class ObjectGetRequest: IRequest
	{
		public int RpcId { get; set; }

		public long Key;

	}

	[Message(InnerOpcode.ObjectGetResponse)]
	public partial class ObjectGetResponse: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long InstanceId;

	}

	[Message(InnerOpcode.R2G_GetLoginKey)]
	public partial class R2G_GetLoginKey: IRequest
	{
		public int RpcId { get; set; }

		public string Account;

	}

	[Message(InnerOpcode.G2R_GetLoginKey)]
	public partial class G2R_GetLoginKey: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long Key;

	}

	[Message(InnerOpcode.G2M_CreateUnit)]
	public partial class G2M_CreateUnit: IRequest
	{
		public int RpcId { get; set; }

		public long PlayerId;

		public long GateSessionId;

	}

	[Message(InnerOpcode.M2G_CreateUnit)]
	public partial class M2G_CreateUnit: IResponse
	{
		public int RpcId { get; set; }

		public int Error { get; set; }

		public string Message { get; set; }

		public long UnitId;

		public int Count;

	}

	[Message(InnerOpcode.G2M_SessionDisconnect)]
	public partial class G2M_SessionDisconnect: IActorMessage
	{
		public int RpcId { get; set; }

		public long ActorId { get; set; }

	}

}
