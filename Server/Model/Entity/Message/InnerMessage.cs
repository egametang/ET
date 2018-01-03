using ProtoBuf;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace Model
{
/// <summary>
/// 用来包装actor消息
/// </summary>
	[Message(Opcode.ActorRequest)]
	[ProtoContract]
	public partial class ActorRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public AMessage AMessage;

	}

/// <summary>
/// actor RPC消息响应
/// </summary>
	[Message(Opcode.ActorResponse)]
	[ProtoContract]
	public partial class ActorResponse:  AResponse
	{
	}

/// <summary>
/// 用来包装actor消息
/// </summary>
	[Message(Opcode.ActorRpcRequest)]
	[ProtoContract]
	public partial class ActorRpcRequest:  ActorRequest
	{
	}

/// <summary>
/// actor RPC消息响应带回应
/// </summary>
	[Message(Opcode.ActorRpcResponse)]
	[ProtoContract]
	public partial class ActorRpcResponse:  ActorResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public AMessage AMessage;

	}

/// <summary>
/// 传送unit
/// </summary>
	[Message(Opcode.M2M_TrasferUnitRequest)]
	[ProtoContract]
	public partial class M2M_TrasferUnitRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public Unit Unit;

	}

	[Message(Opcode.M2M_TrasferUnitResponse)]
	[ProtoContract]
	public partial class M2M_TrasferUnitResponse:  AResponse
	{
	}

	[Message(Opcode.M2A_Reload)]
	[ProtoContract]
	public partial class M2A_Reload:  ARequest
	{
	}

	[Message(Opcode.A2M_Reload)]
	[ProtoContract]
	public partial class A2M_Reload:  AResponse
	{
	}

	[Message(Opcode.G2G_LockRequest)]
	[ProtoContract]
	public partial class G2G_LockRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string Address;

	}

	[Message(Opcode.G2G_LockResponse)]
	[ProtoContract]
	public partial class G2G_LockResponse:  AResponse
	{
	}

	[Message(Opcode.G2G_LockReleaseRequest)]
	[ProtoContract]
	public partial class G2G_LockReleaseRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string Address;

	}

	[Message(Opcode.G2G_LockReleaseResponse)]
	[ProtoContract]
	public partial class G2G_LockReleaseResponse:  AResponse
	{
	}

	[Message(Opcode.DBSaveRequest)]
	[ProtoContract]
	public partial class DBSaveRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public bool NeedCache;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3, IsRequired = true)]
		public Disposer Disposer;

	}

	[Message(Opcode.DBSaveBatchResponse)]
	[ProtoContract]
	public partial class DBSaveBatchResponse:  AResponse
	{
	}

	[Message(Opcode.DBSaveBatchRequest)]
	[ProtoContract]
	public partial class DBSaveBatchRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public bool NeedCache;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3)]
		public List<Disposer> Disposers = new List<Disposer>();

	}

	[Message(Opcode.DBSaveResponse)]
	[ProtoContract]
	public partial class DBSaveResponse:  AResponse
	{
	}

	[Message(Opcode.DBQueryRequest)]
	[ProtoContract]
	public partial class DBQueryRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3, IsRequired = true)]
		public bool NeedCache;

	}

	[Message(Opcode.DBQueryResponse)]
	[ProtoContract]
	public partial class DBQueryResponse:  AResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public Disposer Disposer;

	}

	[Message(Opcode.DBQueryBatchRequest)]
	[ProtoContract]
	public partial class DBQueryBatchRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(2)]
		public List<long> IdList = new List<long>();

		[ProtoMember(3, IsRequired = true)]
		public bool NeedCache;

	}

	[Message(Opcode.DBQueryBatchResponse)]
	[ProtoContract]
	public partial class DBQueryBatchResponse:  AResponse
	{
		[ProtoMember(1)]
		public List<Disposer> Disposers = new List<Disposer>();

	}

	[Message(Opcode.DBQueryJsonRequest)]
	[ProtoContract]
	public partial class DBQueryJsonRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(2, IsRequired = true)]
		public string Json;

		[ProtoMember(3, IsRequired = true)]
		public bool NeedCache;

	}

	[Message(Opcode.DBQueryJsonResponse)]
	[ProtoContract]
	public partial class DBQueryJsonResponse:  AResponse
	{
		[ProtoMember(1)]
		public List<Disposer> Disposers = new List<Disposer>();

	}

	[Message(Opcode.ObjectAddRequest)]
	[ProtoContract]
	public partial class ObjectAddRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public int AppId;

	}

	[Message(Opcode.ObjectAddResponse)]
	[ProtoContract]
	public partial class ObjectAddResponse:  AResponse
	{
	}

	[Message(Opcode.ObjectRemoveRequest)]
	[ProtoContract]
	public partial class ObjectRemoveRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(Opcode.ObjectRemoveResponse)]
	[ProtoContract]
	public partial class ObjectRemoveResponse:  AResponse
	{
	}

	[Message(Opcode.ObjectLockRequest)]
	[ProtoContract]
	public partial class ObjectLockRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public int LockAppId;

		[ProtoMember(3, IsRequired = true)]
		public int Time;

	}

	[Message(Opcode.ObjectLockResponse)]
	[ProtoContract]
	public partial class ObjectLockResponse:  AResponse
	{
	}

	[Message(Opcode.ObjectUnLockRequest)]
	[ProtoContract]
	public partial class ObjectUnLockRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public int UnLockAppId;

		[ProtoMember(3, IsRequired = true)]
		public int AppId;

	}

	[Message(Opcode.ObjectUnLockResponse)]
	[ProtoContract]
	public partial class ObjectUnLockResponse:  AResponse
	{
	}

	[Message(Opcode.ObjectGetRequest)]
	[ProtoContract]
	public partial class ObjectGetRequest:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(Opcode.ObjectGetResponse)]
	[ProtoContract]
	public partial class ObjectGetResponse:  AResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public int AppId;

	}

	[Message(Opcode.R2G_GetLoginKey)]
	[ProtoContract]
	public partial class R2G_GetLoginKey:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string Account;

	}

	[Message(Opcode.G2R_GetLoginKey)]
	[ProtoContract]
	public partial class G2R_GetLoginKey:  AResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(Opcode.G2M_CreateUnit)]
	[ProtoContract]
	public partial class G2M_CreateUnit:  ARequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long PlayerId;

		[ProtoMember(2, IsRequired = true)]
		public long GateSessionId;

	}

	[Message(Opcode.M2G_CreateUnit)]
	[ProtoContract]
	public partial class M2G_CreateUnit:  AResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long UnitId;

		[ProtoMember(2, IsRequired = true)]
		public int Count;

	}

	[BsonKnownTypes(typeof(ActorRpcRequest))]
	public partial class  ActorRequest {}

	[BsonKnownTypes(typeof(ActorRpcResponse))]
	public partial class  ActorResponse {}

	[BsonKnownTypes(typeof(ActorRequest))]
	[BsonKnownTypes(typeof(M2M_TrasferUnitRequest))]
	[BsonKnownTypes(typeof(M2A_Reload))]
	[BsonKnownTypes(typeof(G2G_LockRequest))]
	[BsonKnownTypes(typeof(G2G_LockReleaseRequest))]
	[BsonKnownTypes(typeof(DBSaveRequest))]
	[BsonKnownTypes(typeof(DBSaveBatchRequest))]
	[BsonKnownTypes(typeof(DBQueryRequest))]
	[BsonKnownTypes(typeof(DBQueryBatchRequest))]
	[BsonKnownTypes(typeof(DBQueryJsonRequest))]
	[BsonKnownTypes(typeof(ObjectAddRequest))]
	[BsonKnownTypes(typeof(ObjectRemoveRequest))]
	[BsonKnownTypes(typeof(ObjectLockRequest))]
	[BsonKnownTypes(typeof(ObjectUnLockRequest))]
	[BsonKnownTypes(typeof(ObjectGetRequest))]
	[BsonKnownTypes(typeof(R2G_GetLoginKey))]
	[BsonKnownTypes(typeof(G2M_CreateUnit))]
	public partial class  ARequest {}

	[BsonKnownTypes(typeof(ActorResponse))]
	[BsonKnownTypes(typeof(M2M_TrasferUnitResponse))]
	[BsonKnownTypes(typeof(A2M_Reload))]
	[BsonKnownTypes(typeof(G2G_LockResponse))]
	[BsonKnownTypes(typeof(G2G_LockReleaseResponse))]
	[BsonKnownTypes(typeof(DBSaveBatchResponse))]
	[BsonKnownTypes(typeof(DBSaveResponse))]
	[BsonKnownTypes(typeof(DBQueryResponse))]
	[BsonKnownTypes(typeof(DBQueryBatchResponse))]
	[BsonKnownTypes(typeof(DBQueryJsonResponse))]
	[BsonKnownTypes(typeof(ObjectAddResponse))]
	[BsonKnownTypes(typeof(ObjectRemoveResponse))]
	[BsonKnownTypes(typeof(ObjectLockResponse))]
	[BsonKnownTypes(typeof(ObjectUnLockResponse))]
	[BsonKnownTypes(typeof(ObjectGetResponse))]
	[BsonKnownTypes(typeof(G2R_GetLoginKey))]
	[BsonKnownTypes(typeof(M2G_CreateUnit))]
	public partial class  AResponse {}

}
