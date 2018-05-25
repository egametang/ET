using ProtoBuf;
using ETModel;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace ETHotfix
{
/// <summary>
/// 传送unit
/// </summary>
	[Message(InnerOpcode.M2M_TrasferUnitRequest)]
	[ProtoContract]
	public partial class M2M_TrasferUnitRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public Unit Unit;

	}

	[Message(InnerOpcode.M2M_TrasferUnitResponse)]
	[ProtoContract]
	public partial class M2M_TrasferUnitResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long InstanceId;

	}

	[Message(InnerOpcode.M2A_Reload)]
	[ProtoContract]
	public partial class M2A_Reload: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

	}

	[Message(InnerOpcode.A2M_Reload)]
	[ProtoContract]
	public partial class A2M_Reload: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.G2G_LockRequest)]
	[ProtoContract]
	public partial class G2G_LockRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string Address;

	}

	[Message(InnerOpcode.G2G_LockResponse)]
	[ProtoContract]
	public partial class G2G_LockResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.G2G_LockReleaseRequest)]
	[ProtoContract]
	public partial class G2G_LockReleaseRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string Address;

	}

	[Message(InnerOpcode.G2G_LockReleaseResponse)]
	[ProtoContract]
	public partial class G2G_LockReleaseResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBSaveRequest)]
	[ProtoContract]
	public partial class DBSaveRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public bool NeedCache;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3, IsRequired = true)]
		public ComponentWithId Component;

	}

	[Message(InnerOpcode.DBSaveBatchResponse)]
	[ProtoContract]
	public partial class DBSaveBatchResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBSaveBatchRequest)]
	[ProtoContract]
	public partial class DBSaveBatchRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public bool NeedCache;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3)]
		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.DBSaveResponse)]
	[ProtoContract]
	public partial class DBSaveResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.DBQueryRequest)]
	[ProtoContract]
	public partial class DBQueryRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3, IsRequired = true)]
		public bool NeedCache;

	}

	[Message(InnerOpcode.DBQueryResponse)]
	[ProtoContract]
	public partial class DBQueryResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public ComponentWithId Component;

	}

	[Message(InnerOpcode.DBQueryBatchRequest)]
	[ProtoContract]
	public partial class DBQueryBatchRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(2)]
		public List<long> IdList = new List<long>();

		[ProtoMember(3, IsRequired = true)]
		public bool NeedCache;

	}

	[Message(InnerOpcode.DBQueryBatchResponse)]
	[ProtoContract]
	public partial class DBQueryBatchResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.DBQueryJsonRequest)]
	[ProtoContract]
	public partial class DBQueryJsonRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(2, IsRequired = true)]
		public string Json;

	}

	[Message(InnerOpcode.DBQueryJsonResponse)]
	[ProtoContract]
	public partial class DBQueryJsonResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public List<ComponentWithId> Components = new List<ComponentWithId>();

	}

	[Message(InnerOpcode.ObjectAddRequest)]
	[ProtoContract]
	public partial class ObjectAddRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public long InstanceId;

	}

	[Message(InnerOpcode.ObjectAddResponse)]
	[ProtoContract]
	public partial class ObjectAddResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectRemoveRequest)]
	[ProtoContract]
	public partial class ObjectRemoveRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(InnerOpcode.ObjectRemoveResponse)]
	[ProtoContract]
	public partial class ObjectRemoveResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectLockRequest)]
	[ProtoContract]
	public partial class ObjectLockRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public long InstanceId;

		[ProtoMember(3, IsRequired = true)]
		public int Time;

	}

	[Message(InnerOpcode.ObjectLockResponse)]
	[ProtoContract]
	public partial class ObjectLockResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectUnLockRequest)]
	[ProtoContract]
	public partial class ObjectUnLockRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public long OldInstanceId;

		[ProtoMember(3, IsRequired = true)]
		public long InstanceId;

	}

	[Message(InnerOpcode.ObjectUnLockResponse)]
	[ProtoContract]
	public partial class ObjectUnLockResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

	}

	[Message(InnerOpcode.ObjectGetRequest)]
	[ProtoContract]
	public partial class ObjectGetRequest: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(InnerOpcode.ObjectGetResponse)]
	[ProtoContract]
	public partial class ObjectGetResponse: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long InstanceId;

	}

	[Message(InnerOpcode.R2G_GetLoginKey)]
	[ProtoContract]
	public partial class R2G_GetLoginKey: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public string Account;

	}

	[Message(InnerOpcode.G2R_GetLoginKey)]
	[ProtoContract]
	public partial class G2R_GetLoginKey: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(InnerOpcode.G2M_CreateUnit)]
	[ProtoContract]
	public partial class G2M_CreateUnit: IRequest
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long PlayerId;

		[ProtoMember(2, IsRequired = true)]
		public long GateSessionId;

	}

	[Message(InnerOpcode.M2G_CreateUnit)]
	[ProtoContract]
	public partial class M2G_CreateUnit: IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(92, IsRequired = true)]
		public string Message { get; set; }

		[ProtoMember(1, IsRequired = true)]
		public long UnitId;

		[ProtoMember(2, IsRequired = true)]
		public int Count;

	}

	[Message(InnerOpcode.G2M_SessionDisconnect)]
	[ProtoContract]
	public partial class G2M_SessionDisconnect: IActorMessage
	{
		[ProtoMember(90, IsRequired = true)]
		public int RpcId { get; set; }

		[ProtoMember(93, IsRequired = true)]
		public long ActorId { get; set; }

	}

}
