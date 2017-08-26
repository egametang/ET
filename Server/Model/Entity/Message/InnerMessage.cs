using System.Collections.Generic;using MongoDB.Bson.Serialization.Attributes;


// 服务器内部消息 Opcode从10000开始
namespace Model{	[Message(Opcode.M2A_Reload)]	[BsonIgnoreExtraElements]	public class M2A_Reload : ARequest	{	}

	[Message(Opcode.A2M_Reload)]	[BsonIgnoreExtraElements]	public class A2M_Reload : AResponse	{	}

	[Message(Opcode.G2G_LockRequest)]	[BsonIgnoreExtraElements]	public class G2G_LockRequest : ARequest	{		public long Id;		public string Address;	}

	[Message(Opcode.G2G_LockResponse)]	[BsonIgnoreExtraElements]
	public class G2G_LockResponse : AResponse	{	}

	[Message(Opcode.G2G_LockReleaseRequest)]	[BsonIgnoreExtraElements]	public class G2G_LockReleaseRequest : ARequest	{		public long Id;		public string Address;	}

	[Message(Opcode.G2G_LockReleaseResponse)]	[BsonIgnoreExtraElements]	public class G2G_LockReleaseResponse : AResponse	{	}
	[Message(Opcode.DBSaveRequest)]	[BsonIgnoreExtraElements]	public class DBSaveRequest : ARequest	{		public bool NeedCache = true;
		public string CollectionName = "";

		public Entity Entity;	}


	[Message(Opcode.DBSaveBatchResponse)]	[BsonIgnoreExtraElements]	public class DBSaveBatchResponse : AResponse	{	}

	[Message(Opcode.DBSaveBatchRequest)]	[BsonIgnoreExtraElements]	public class DBSaveBatchRequest : ARequest	{		public bool NeedCache = true;		public string CollectionName = "";		public List<Entity> Entitys = new List<Entity>();	}

	[Message(Opcode.DBSaveResponse)]	[BsonIgnoreExtraElements]	public class DBSaveResponse : AResponse	{	}

	[Message(Opcode.DBQueryRequest)]	[BsonIgnoreExtraElements]	public class DBQueryRequest : ARequest	{		public long Id;		public string CollectionName { get; set; }		public bool NeedCache = true;	}

	[Message(Opcode.DBQueryResponse)]	[BsonIgnoreExtraElements]	public class DBQueryResponse : AResponse	{		public Entity Entity;	}

	[Message(Opcode.DBQueryBatchRequest)]	[BsonIgnoreExtraElements]	public class DBQueryBatchRequest : ARequest	{		public string CollectionName { get; set; }		public List<long> IdList { get; set; }		public bool NeedCache = true;	}

	[Message(Opcode.DBQueryBatchResponse)]	[BsonIgnoreExtraElements]	public class DBQueryBatchResponse : AResponse	{		public List<Entity> Entitys;	}

	[Message(Opcode.DBQueryJsonRequest)]	[BsonIgnoreExtraElements]	public class DBQueryJsonRequest : ARequest	{		public string CollectionName { get; set; }		public string Json { get; set; }		public bool NeedCache = true;	}

	[Message(Opcode.DBQueryJsonResponse)]	[BsonIgnoreExtraElements]	public class DBQueryJsonResponse : AResponse	{		public List<Entity> Entitys;	}

	[Message(Opcode.ObjectAddRequest)]	[BsonIgnoreExtraElements]	public class ObjectAddRequest : ARequest	{		public long Key { get; set; }		public int AppId { get; set; }	}

	[Message(Opcode.ObjectAddResponse)]	[BsonIgnoreExtraElements]	public class ObjectAddResponse : AResponse	{	}

	[Message(Opcode.ObjectRemoveRequest)]	[BsonIgnoreExtraElements]	public class ObjectRemoveRequest : ARequest	{		public long Key { get; set; }	}

	[Message(Opcode.ObjectRemoveResponse)]	[BsonIgnoreExtraElements]	public class ObjectRemoveResponse : AResponse	{	}

	[Message(Opcode.ObjectLockRequest)]	[BsonIgnoreExtraElements]	public class ObjectLockRequest : ARequest	{		public long Key { get; set; }		public int LockAppId { get; set; }		public int AppId { get; set; }		public int Time { get; set; }	}

	[Message(Opcode.ObjectLockResponse)]	[BsonIgnoreExtraElements]	public class ObjectLockResponse : AResponse	{	}

	[Message(Opcode.ObjectUnLockRequest)]	[BsonIgnoreExtraElements]	public class ObjectUnLockRequest : ARequest	{		public long Key { get; set; }		public int LockAppId { get; set; }		public int AppId { get; set; }	}

	[Message(Opcode.ObjectUnLockResponse)]	[BsonIgnoreExtraElements]	public class ObjectUnLockResponse : AResponse	{	}

	[Message(Opcode.ObjectGetRequest)]	[BsonIgnoreExtraElements]	public class ObjectGetRequest : ARequest	{		public long Key { get; set; }	}

	[Message(Opcode.ObjectGetResponse)]	[BsonIgnoreExtraElements]	public class ObjectGetResponse : AResponse	{		public int AppId { get; set; }	}	[Message(Opcode.R2G_GetLoginKey)]	[BsonIgnoreExtraElements]	public class R2G_GetLoginKey : ARequest	{		public string Account;	}	[Message(Opcode.G2R_GetLoginKey)]	[BsonIgnoreExtraElements]	public class G2R_GetLoginKey : AResponse	{		public long Key;		public G2R_GetLoginKey()		{		}		public G2R_GetLoginKey(long key)		{			this.Key = key;		}	}	[Message(Opcode.G2M_CreateUnit)]	[BsonIgnoreExtraElements]	public class G2M_CreateUnit : ARequest	{		public long PlayerId;	}	[Message(Opcode.M2G_CreateUnit)]	[BsonIgnoreExtraElements]	public class M2G_CreateUnit : AResponse	{		public long UnitId;	}}