using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

// 服务器内部消息 Opcode从10000开始
namespace Model
{
	public abstract partial class AMessage
	{
	}

    [BsonKnownTypes(typeof(C2M_Reload))]
    [BsonKnownTypes(typeof(C2R_Ping))]
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
    public abstract partial class ARequest : AMessage
	{
	}

    [BsonKnownTypes(typeof(M2C_Reload))]
    [BsonKnownTypes(typeof(R2C_Ping))]
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
    public abstract partial class AResponse : AMessage
	{
	}

	public abstract partial class AActorMessage : AMessage
	{
	}
	
	public abstract partial class AActorRequest : ARequest
	{
	}
	
	public abstract partial class AActorResponse : AResponse
	{
	}
	


	/// <summary>
	/// 用来包装actor消息
	/// </summary>
    [Message(Opcode.ActorRequest)]
	[BsonIgnoreExtraElements]
	public class ActorRequest : ARequest
	{
		public long Id { get; set; }

		public AMessage AMessage { get; set; }
	}

	/// <summary>
	/// actor RPC消息响应
	/// </summary>
	[Message(Opcode.ActorResponse)]
	[BsonIgnoreExtraElements]
	public class ActorResponse : AResponse
	{
	}

	/// <summary>
	/// 用来包装actor消息
	/// </summary>
	[Message(Opcode.ActorRpcRequest)]
	[BsonIgnoreExtraElements]
	public class ActorRpcRequest : ActorRequest
	{
	}

	/// <summary>
	/// actor RPC消息响应带回应
	/// </summary>
    [Message(Opcode.ActorRpcResponse)]
	[BsonIgnoreExtraElements]
	public class ActorRpcResponse : ActorResponse
	{
		public AMessage AMessage { get; set; }
	}


	/// <summary>
	/// 传送unit
	/// </summary>
    [Message(Opcode.M2M_TrasferUnitRequest)]
	[BsonIgnoreExtraElements]
	public class M2M_TrasferUnitRequest : ARequest
	{
		public Unit Unit;
	}
	
	[Message(Opcode.M2M_TrasferUnitResponse)]
	[BsonIgnoreExtraElements]
	public class M2M_TrasferUnitResponse : AResponse
	{
	}
	


	[Message(Opcode.M2A_Reload)]
	[BsonIgnoreExtraElements]
	public class M2A_Reload : ARequest
	{
	}


	[Message(Opcode.A2M_Reload)]
	[BsonIgnoreExtraElements]
	public class A2M_Reload : AResponse
	{
	}


	[Message(Opcode.G2G_LockRequest)]
	[BsonIgnoreExtraElements]
	public class G2G_LockRequest : ARequest
	{
		public long Id;
		public string Address;
	}


	[Message(Opcode.G2G_LockResponse)]
	[BsonIgnoreExtraElements]
	public class G2G_LockResponse : AResponse
	{
	}

	[Message(Opcode.G2G_LockReleaseRequest)]
	[BsonIgnoreExtraElements]
	public class G2G_LockReleaseRequest : ARequest
	{
		public long Id;
		public string Address;
	}


	[Message(Opcode.G2G_LockReleaseResponse)]
	[BsonIgnoreExtraElements]
	public class G2G_LockReleaseResponse : AResponse
	{
	}

	[Message(Opcode.DBSaveRequest)]
	[BsonIgnoreExtraElements]
	public class DBSaveRequest : ARequest
	{
		public bool NeedCache = true;

		public string CollectionName = "";

		public Entity Entity;
	}



	[Message(Opcode.DBSaveBatchResponse)]
	[BsonIgnoreExtraElements]
	public class DBSaveBatchResponse : AResponse
	{
	}


	[Message(Opcode.DBSaveBatchRequest)]
	[BsonIgnoreExtraElements]
	public class DBSaveBatchRequest : ARequest
	{
		public bool NeedCache = true;
		public string CollectionName = "";
		public List<Entity> Entitys = new List<Entity>();
	}

	[Message(Opcode.DBSaveResponse)]
	[BsonIgnoreExtraElements]
	public class DBSaveResponse : AResponse
	{
	}


	[Message(Opcode.DBQueryRequest)]
	[BsonIgnoreExtraElements]
	public class DBQueryRequest : ARequest
	{
		public long Id;
		public string CollectionName { get; set; }
		public bool NeedCache = true;
	}


	[Message(Opcode.DBQueryResponse)]
	[BsonIgnoreExtraElements]
	public class DBQueryResponse : AResponse
	{
		public Entity Entity;
	}


	[Message(Opcode.DBQueryBatchRequest)]
	[BsonIgnoreExtraElements]
	public class DBQueryBatchRequest : ARequest
	{
		public string CollectionName { get; set; }
		public List<long> IdList { get; set; }
		public bool NeedCache = true;
	}

	[Message(Opcode.DBQueryBatchResponse)]
	[BsonIgnoreExtraElements]
	public class DBQueryBatchResponse : AResponse
	{
		public List<Entity> Entitys;
	}


	[Message(Opcode.DBQueryJsonRequest)]
	[BsonIgnoreExtraElements]
	public class DBQueryJsonRequest : ARequest
	{
		public string CollectionName { get; set; }
		public string Json { get; set; }
		public bool NeedCache = true;
	}

	[Message(Opcode.DBQueryJsonResponse)]
	[BsonIgnoreExtraElements]
	public class DBQueryJsonResponse : AResponse
	{
		public List<Entity> Entitys;
	}

	[Message(Opcode.ObjectAddRequest)]
	[BsonIgnoreExtraElements]
	public class ObjectAddRequest : ARequest
	{
		public long Key { get; set; }
		public int AppId { get; set; }
	}

	[Message(Opcode.ObjectAddResponse)]
	[BsonIgnoreExtraElements]
	public class ObjectAddResponse : AResponse
	{
	}

	[Message(Opcode.ObjectRemoveRequest)]
	[BsonIgnoreExtraElements]
	public class ObjectRemoveRequest : ARequest
	{
		public long Key { get; set; }
	}

	[Message(Opcode.ObjectRemoveResponse)]
	[BsonIgnoreExtraElements]
	public class ObjectRemoveResponse : AResponse
	{
	}

	[Message(Opcode.ObjectLockRequest)]
	[BsonIgnoreExtraElements]
	public class ObjectLockRequest : ARequest
	{
		public long Key { get; set; }
		public int LockAppId { get; set; }
		public int Time { get; set; }
	}

	[Message(Opcode.ObjectLockResponse)]
	[BsonIgnoreExtraElements]
	public class ObjectLockResponse : AResponse
	{
	}

	[Message(Opcode.ObjectUnLockRequest)]
	[BsonIgnoreExtraElements]
	public class ObjectUnLockRequest : ARequest
	{
		public long Key { get; set; }
		public int UnLockAppId { get; set; }
		public int AppId { get; set; }
	}

	[Message(Opcode.ObjectUnLockResponse)]
	[BsonIgnoreExtraElements]
	public class ObjectUnLockResponse : AResponse
	{
	}

	[Message(Opcode.ObjectGetRequest)]
	[BsonIgnoreExtraElements]
	public class ObjectGetRequest : ARequest
	{
		public long Key { get; set; }
	}

	[Message(Opcode.ObjectGetResponse)]
	[BsonIgnoreExtraElements]
	public class ObjectGetResponse : AResponse
	{
		public int AppId { get; set; }
	}


	[Message(Opcode.R2G_GetLoginKey)]
	[BsonIgnoreExtraElements]
	public class R2G_GetLoginKey : ARequest
	{
		public string Account;
	}


	[Message(Opcode.G2R_GetLoginKey)]
	[BsonIgnoreExtraElements]
	public class G2R_GetLoginKey : AResponse
	{
		public long Key;

		public G2R_GetLoginKey()
		{
		}

		public G2R_GetLoginKey(long key)
		{
			this.Key = key;
		}
	}


	[Message(Opcode.G2M_CreateUnit)]
	[BsonIgnoreExtraElements]
	public class G2M_CreateUnit : ARequest
	{
		public long PlayerId;
		public long GateSessionId;
	}


	[Message(Opcode.M2G_CreateUnit)]
	[BsonIgnoreExtraElements]
	public class M2G_CreateUnit : AResponse
	{
		public long UnitId;
		public int Count;
	}

    [Message(Opcode.DBInsertRequest)]
    [BsonIgnoreExtraElements]
    public class DBInsertRequest : ARequest
    {
        public bool NeedCache;

        public string CollectionName;

        public Entity Entity;
    }

    [Message(Opcode.DBInsertResponse)]
    [BsonIgnoreExtraElements]
    public class DBInsertResponse : AResponse
    {

    }

    [Message(Opcode.DBInsertBatchRequest)]
    [BsonIgnoreExtraElements]
    public class DBInsertBatchRequest : ARequest
    {
        public bool NeedCache;

        public string CollectionName;

        public List<Entity> Entitys;
    }

    [Message(Opcode.DBInsertBatchResponse)]
    [BsonIgnoreExtraElements]
    public class DBInsertBatchResponse : AResponse
    {

    }

    [Message(Opcode.DBDeleteRequest)]
    [BsonIgnoreExtraElements]
    public class DBDeleteRequest : ARequest
    {
        public string CollectionName;

        public string Filter;
    }

    [Message(Opcode.DBDeleteResponse)]
    [BsonIgnoreExtraElements]
    public class DBDeleteResponse : AResponse
    {

    }

    [Message(Opcode.DBDeleteBatchRequest)]
    [BsonIgnoreExtraElements]
    public class DBDeleteBatchRequest : ARequest
    {
        public string CollectionName;

        public string Filter;
    }

    [Message(Opcode.DBDeleteBatchResponse)]
    [BsonIgnoreExtraElements]
    public class DBDeleteBatchResponse : AResponse
    {

    }

    [Message(Opcode.DBUpdateRequest)]
    [BsonIgnoreExtraElements]
    public class DBUpdateRequest : ARequest
    {
        public string CollectionName;

        public DBUpdateOptions Options;
    }

    [Message(Opcode.DBUpdateResponse)]
    [BsonIgnoreExtraElements]
    public class DBUpdateResponse : AResponse
    {

    }

    [Message(Opcode.DBUpdateBatchRequest)]
    [BsonIgnoreExtraElements]
    public class DBUpdateBatchRequest : ARequest
    {
        public string CollectionName;

        public DBUpdateOptions Options;
    }

    [Message(Opcode.DBUpdateBatchResponse)]
    [BsonIgnoreExtraElements]
    public class DBUpdateBatchResponse : AResponse
    {

    }

    [Message(Opcode.DBFindRequest)]
    [BsonIgnoreExtraElements]
    public class DBFindRequest : ARequest
    {
        public string CollectionName;

        public bool NeedCache;

        public DBFindOptions Options;
    }

    [Message(Opcode.DBFindResponse)]
    [BsonIgnoreExtraElements]
    public class DBFindResponse : AResponse
    {
        public Entity Result;
    }

    [Message(Opcode.DBFindBatchRequest)]
    [BsonIgnoreExtraElements]
    public class DBFindBatchRequest : ARequest
    {
        public string CollectionName;

        public bool NeedCache;

        public DBFindOptions Options;
    }

    [Message(Opcode.DBFindBatchResponse)]
    [BsonIgnoreExtraElements]
    public class DBFindBatchResponse : AResponse
    {
        public List<Entity> Result;
    }

    [Message(Opcode.DBFindAndDeleteRequest)]
    [BsonIgnoreExtraElements]
    public class DBFindAndDeleteRequest : ARequest
    {
        public string CollectionName;

        public DBFindAndDeleteOptions Options;
    }

    [Message(Opcode.DBFindAndDeleteResponse)]
    [BsonIgnoreExtraElements]
    public class DBFindAndDeleteResponse : AResponse
    {
        public Entity Result;
    }
}