using ProtoBuf;
using System.Collections.Generic;
using Model;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	/// <summary>
	/// 传送unit
	/// </summary>
	[Message(Opcode.M2M_TrasferUnitRequest)]
	[ProtoContract]
	public partial class M2M_TrasferUnitRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public Unit Unit;

	}

	[Message(Opcode.M2M_TrasferUnitResponse)]
	[ProtoContract]
	public partial class M2M_TrasferUnitResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.M2A_Reload)]
	[ProtoContract]
	public partial class M2A_Reload: MessageObject, IRequest
	{
	}

	[Message(Opcode.A2M_Reload)]
	[ProtoContract]
	public partial class A2M_Reload: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.G2G_LockRequest)]
	[ProtoContract]
	public partial class G2G_LockRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string Address;

	}

	[Message(Opcode.G2G_LockResponse)]
	[ProtoContract]
	public partial class G2G_LockResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.G2G_LockReleaseRequest)]
	[ProtoContract]
	public partial class G2G_LockReleaseRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Id;

		[ProtoMember(2, IsRequired = true)]
		public string Address;

	}

	[Message(Opcode.G2G_LockReleaseResponse)]
	[ProtoContract]
	public partial class G2G_LockReleaseResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.DBSaveRequest)]
	[ProtoContract]
	public partial class DBSaveRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public bool NeedCache;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3, IsRequired = true)]
		public Component Disposer;

	}

	[Message(Opcode.DBSaveBatchResponse)]
	[ProtoContract]
	public partial class DBSaveBatchResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.DBSaveBatchRequest)]
	[ProtoContract]
	public partial class DBSaveBatchRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public bool NeedCache;

		[ProtoMember(2, IsRequired = true)]
		public string CollectionName;

		[ProtoMember(3)]
		public List<Component> Disposers = new List<Component>();

	}

	[Message(Opcode.DBSaveResponse)]
	[ProtoContract]
	public partial class DBSaveResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.DBQueryRequest)]
	[ProtoContract]
	public partial class DBQueryRequest: MessageObject, IRequest
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
	public partial class DBQueryResponse: MessageObject, IResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public Component Disposer;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.DBQueryBatchRequest)]
	[ProtoContract]
	public partial class DBQueryBatchRequest: MessageObject, IRequest
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
	public partial class DBQueryBatchResponse: MessageObject, IResponse
	{
		[ProtoMember(1)]
		public List<Component> Disposers = new List<Component>();

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.DBQueryJsonRequest)]
	[ProtoContract]
	public partial class DBQueryJsonRequest: MessageObject, IRequest
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
	public partial class DBQueryJsonResponse: MessageObject, IResponse
	{
		[ProtoMember(1)]
		public List<Component> Disposers = new List<Component>();

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.ObjectAddRequest)]
	[ProtoContract]
	public partial class ObjectAddRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(2, IsRequired = true)]
		public int AppId;

	}

	[Message(Opcode.ObjectAddResponse)]
	[ProtoContract]
	public partial class ObjectAddResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.ObjectRemoveRequest)]
	[ProtoContract]
	public partial class ObjectRemoveRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

	}

	[Message(Opcode.ObjectRemoveResponse)]
	[ProtoContract]
	public partial class ObjectRemoveResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.ObjectLockRequest)]
	[ProtoContract]
	public partial class ObjectLockRequest: MessageObject, IRequest
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
	public partial class ObjectLockResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.ObjectUnLockRequest)]
	[ProtoContract]
	public partial class ObjectUnLockRequest: MessageObject, IRequest
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
	public partial class ObjectUnLockResponse: MessageObject, IResponse
	{
		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.ObjectGetRequest)]
	[ProtoContract]
	public partial class ObjectGetRequest: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.ObjectGetResponse)]
	[ProtoContract]
	public partial class ObjectGetResponse: MessageObject, IResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public int AppId;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.R2G_GetLoginKey)]
	[ProtoContract]
	public partial class R2G_GetLoginKey: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public string Account;

	}

	[Message(Opcode.G2R_GetLoginKey)]
	[ProtoContract]
	public partial class G2R_GetLoginKey: MessageObject, IResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long Key;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[Message(Opcode.G2M_CreateUnit)]
	[ProtoContract]
	public partial class G2M_CreateUnit: MessageObject, IRequest
	{
		[ProtoMember(1, IsRequired = true)]
		public long PlayerId;

		[ProtoMember(2, IsRequired = true)]
		public long GateSessionId;

	}

	[Message(Opcode.M2G_CreateUnit)]
	[ProtoContract]
	public partial class M2G_CreateUnit: MessageObject, IResponse
	{
		[ProtoMember(1, IsRequired = true)]
		public long UnitId;

		[ProtoMember(2, IsRequired = true)]
		public int Count;

		[ProtoMember(90, IsRequired = true)]
		public int Error { get; set; }

		[ProtoMember(91, IsRequired = true)]
		public string Message { get; set; }
	}

	[BsonKnownTypes(typeof(FrameMessage))]
	[BsonKnownTypes(typeof(Actor_CreateUnits))]
	[BsonKnownTypes(typeof(Actor_TransferRequest))]
	public partial class MessageObject
	{
	}
}