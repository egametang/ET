using ET;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
// using
	[ResponseType(nameof(ObjectQueryResponse))]
	[Message(InnerMessage.ObjectQueryRequest)]
	[MemoryPackable]
	public partial class ObjectQueryRequest: MessageObject, IActorRequest
	{
		public static ObjectQueryRequest Create(bool isFromPool = false) { return !isFromPool? new ObjectQueryRequest() : NetServices.Instance.FetchMessage(typeof(ObjectQueryRequest)) as ObjectQueryRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[MemoryPackOrder(2)]
		public long InstanceId { get; set; }

	}

	[ResponseType(nameof(A2M_Reload))]
	[Message(InnerMessage.M2A_Reload)]
	[MemoryPackable]
	public partial class M2A_Reload: MessageObject, IActorRequest
	{
		public static M2A_Reload Create(bool isFromPool = false) { return !isFromPool? new M2A_Reload() : NetServices.Instance.FetchMessage(typeof(M2A_Reload)) as M2A_Reload; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.A2M_Reload)]
	[MemoryPackable]
	public partial class A2M_Reload: MessageObject, IActorResponse
	{
		public static A2M_Reload Create(bool isFromPool = false) { return !isFromPool? new A2M_Reload() : NetServices.Instance.FetchMessage(typeof(A2M_Reload)) as A2M_Reload; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2G_LockResponse))]
	[Message(InnerMessage.G2G_LockRequest)]
	[MemoryPackable]
	public partial class G2G_LockRequest: MessageObject, IActorRequest
	{
		public static G2G_LockRequest Create(bool isFromPool = false) { return !isFromPool? new G2G_LockRequest() : NetServices.Instance.FetchMessage(typeof(G2G_LockRequest)) as G2G_LockRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[MemoryPackOrder(2)]
		public string Address { get; set; }

	}

	[Message(InnerMessage.G2G_LockResponse)]
	[MemoryPackable]
	public partial class G2G_LockResponse: MessageObject, IActorResponse
	{
		public static G2G_LockResponse Create(bool isFromPool = false) { return !isFromPool? new G2G_LockResponse() : NetServices.Instance.FetchMessage(typeof(G2G_LockResponse)) as G2G_LockResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2G_LockReleaseResponse))]
	[Message(InnerMessage.G2G_LockReleaseRequest)]
	[MemoryPackable]
	public partial class G2G_LockReleaseRequest: MessageObject, IActorRequest
	{
		public static G2G_LockReleaseRequest Create(bool isFromPool = false) { return !isFromPool? new G2G_LockReleaseRequest() : NetServices.Instance.FetchMessage(typeof(G2G_LockReleaseRequest)) as G2G_LockReleaseRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[MemoryPackOrder(2)]
		public string Address { get; set; }

	}

	[Message(InnerMessage.G2G_LockReleaseResponse)]
	[MemoryPackable]
	public partial class G2G_LockReleaseResponse: MessageObject, IActorResponse
	{
		public static G2G_LockReleaseResponse Create(bool isFromPool = false) { return !isFromPool? new G2G_LockReleaseResponse() : NetServices.Instance.FetchMessage(typeof(G2G_LockReleaseResponse)) as G2G_LockReleaseResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectAddResponse))]
	[Message(InnerMessage.ObjectAddRequest)]
	[MemoryPackable]
	public partial class ObjectAddRequest: MessageObject, IActorRequest
	{
		public static ObjectAddRequest Create(bool isFromPool = false) { return !isFromPool? new ObjectAddRequest() : NetServices.Instance.FetchMessage(typeof(ObjectAddRequest)) as ObjectAddRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

		[MemoryPackOrder(3)]
		public ActorId ActorId { get; set; }

	}

	[Message(InnerMessage.ObjectAddResponse)]
	[MemoryPackable]
	public partial class ObjectAddResponse: MessageObject, IActorResponse
	{
		public static ObjectAddResponse Create(bool isFromPool = false) { return !isFromPool? new ObjectAddResponse() : NetServices.Instance.FetchMessage(typeof(ObjectAddResponse)) as ObjectAddResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectLockResponse))]
	[Message(InnerMessage.ObjectLockRequest)]
	[MemoryPackable]
	public partial class ObjectLockRequest: MessageObject, IActorRequest
	{
		public static ObjectLockRequest Create(bool isFromPool = false) { return !isFromPool? new ObjectLockRequest() : NetServices.Instance.FetchMessage(typeof(ObjectLockRequest)) as ObjectLockRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

		[MemoryPackOrder(3)]
		public ActorId ActorId { get; set; }

		[MemoryPackOrder(4)]
		public int Time { get; set; }

	}

	[Message(InnerMessage.ObjectLockResponse)]
	[MemoryPackable]
	public partial class ObjectLockResponse: MessageObject, IActorResponse
	{
		public static ObjectLockResponse Create(bool isFromPool = false) { return !isFromPool? new ObjectLockResponse() : NetServices.Instance.FetchMessage(typeof(ObjectLockResponse)) as ObjectLockResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectUnLockResponse))]
	[Message(InnerMessage.ObjectUnLockRequest)]
	[MemoryPackable]
	public partial class ObjectUnLockRequest: MessageObject, IActorRequest
	{
		public static ObjectUnLockRequest Create(bool isFromPool = false) { return !isFromPool? new ObjectUnLockRequest() : NetServices.Instance.FetchMessage(typeof(ObjectUnLockRequest)) as ObjectUnLockRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

		[MemoryPackOrder(3)]
		public ActorId OldActorId { get; set; }

		[MemoryPackOrder(4)]
		public ActorId NewActorId { get; set; }

	}

	[Message(InnerMessage.ObjectUnLockResponse)]
	[MemoryPackable]
	public partial class ObjectUnLockResponse: MessageObject, IActorResponse
	{
		public static ObjectUnLockResponse Create(bool isFromPool = false) { return !isFromPool? new ObjectUnLockResponse() : NetServices.Instance.FetchMessage(typeof(ObjectUnLockResponse)) as ObjectUnLockResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectRemoveResponse))]
	[Message(InnerMessage.ObjectRemoveRequest)]
	[MemoryPackable]
	public partial class ObjectRemoveRequest: MessageObject, IActorRequest
	{
		public static ObjectRemoveRequest Create(bool isFromPool = false) { return !isFromPool? new ObjectRemoveRequest() : NetServices.Instance.FetchMessage(typeof(ObjectRemoveRequest)) as ObjectRemoveRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

	}

	[Message(InnerMessage.ObjectRemoveResponse)]
	[MemoryPackable]
	public partial class ObjectRemoveResponse: MessageObject, IActorResponse
	{
		public static ObjectRemoveResponse Create(bool isFromPool = false) { return !isFromPool? new ObjectRemoveResponse() : NetServices.Instance.FetchMessage(typeof(ObjectRemoveResponse)) as ObjectRemoveResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectGetResponse))]
	[Message(InnerMessage.ObjectGetRequest)]
	[MemoryPackable]
	public partial class ObjectGetRequest: MessageObject, IActorRequest
	{
		public static ObjectGetRequest Create(bool isFromPool = false) { return !isFromPool? new ObjectGetRequest() : NetServices.Instance.FetchMessage(typeof(ObjectGetRequest)) as ObjectGetRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

	}

	[Message(InnerMessage.ObjectGetResponse)]
	[MemoryPackable]
	public partial class ObjectGetResponse: MessageObject, IActorResponse
	{
		public static ObjectGetResponse Create(bool isFromPool = false) { return !isFromPool? new ObjectGetResponse() : NetServices.Instance.FetchMessage(typeof(ObjectGetResponse)) as ObjectGetResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public int Type { get; set; }

		[MemoryPackOrder(4)]
		public ActorId ActorId { get; set; }

	}

	[ResponseType(nameof(G2R_GetLoginKey))]
	[Message(InnerMessage.R2G_GetLoginKey)]
	[MemoryPackable]
	public partial class R2G_GetLoginKey: MessageObject, IActorRequest
	{
		public static R2G_GetLoginKey Create(bool isFromPool = false) { return !isFromPool? new R2G_GetLoginKey() : NetServices.Instance.FetchMessage(typeof(R2G_GetLoginKey)) as R2G_GetLoginKey; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Account { get; set; }

	}

	[Message(InnerMessage.G2R_GetLoginKey)]
	[MemoryPackable]
	public partial class G2R_GetLoginKey: MessageObject, IActorResponse
	{
		public static G2R_GetLoginKey Create(bool isFromPool = false) { return !isFromPool? new G2R_GetLoginKey() : NetServices.Instance.FetchMessage(typeof(G2R_GetLoginKey)) as G2R_GetLoginKey; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long Key { get; set; }

		[MemoryPackOrder(4)]
		public long GateId { get; set; }

	}

	[Message(InnerMessage.G2M_SessionDisconnect)]
	[MemoryPackable]
	public partial class G2M_SessionDisconnect: MessageObject, IActorLocationMessage
	{
		public static G2M_SessionDisconnect Create(bool isFromPool = false) { return !isFromPool? new G2M_SessionDisconnect() : NetServices.Instance.FetchMessage(typeof(G2M_SessionDisconnect)) as G2M_SessionDisconnect; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.ObjectQueryResponse)]
	[MemoryPackable]
	public partial class ObjectQueryResponse: MessageObject, IActorResponse
	{
		public static ObjectQueryResponse Create(bool isFromPool = false) { return !isFromPool? new ObjectQueryResponse() : NetServices.Instance.FetchMessage(typeof(ObjectQueryResponse)) as ObjectQueryResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public byte[] Entity { get; set; }

	}

	[ResponseType(nameof(M2M_UnitTransferResponse))]
	[Message(InnerMessage.M2M_UnitTransferRequest)]
	[MemoryPackable]
	public partial class M2M_UnitTransferRequest: MessageObject, IActorRequest
	{
		public static M2M_UnitTransferRequest Create(bool isFromPool = false) { return !isFromPool? new M2M_UnitTransferRequest() : NetServices.Instance.FetchMessage(typeof(M2M_UnitTransferRequest)) as M2M_UnitTransferRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public ActorId OldActorId { get; set; }

		[MemoryPackOrder(2)]
		public byte[] Unit { get; set; }

		[MemoryPackOrder(3)]
		public List<byte[]> Entitys { get; set; }

	}

	[Message(InnerMessage.M2M_UnitTransferResponse)]
	[MemoryPackable]
	public partial class M2M_UnitTransferResponse: MessageObject, IActorResponse
	{
		public static M2M_UnitTransferResponse Create(bool isFromPool = false) { return !isFromPool? new M2M_UnitTransferResponse() : NetServices.Instance.FetchMessage(typeof(M2M_UnitTransferResponse)) as M2M_UnitTransferResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	public static class InnerMessage
	{
		 public const ushort ObjectQueryRequest = 20002;
		 public const ushort M2A_Reload = 20003;
		 public const ushort A2M_Reload = 20004;
		 public const ushort G2G_LockRequest = 20005;
		 public const ushort G2G_LockResponse = 20006;
		 public const ushort G2G_LockReleaseRequest = 20007;
		 public const ushort G2G_LockReleaseResponse = 20008;
		 public const ushort ObjectAddRequest = 20009;
		 public const ushort ObjectAddResponse = 20010;
		 public const ushort ObjectLockRequest = 20011;
		 public const ushort ObjectLockResponse = 20012;
		 public const ushort ObjectUnLockRequest = 20013;
		 public const ushort ObjectUnLockResponse = 20014;
		 public const ushort ObjectRemoveRequest = 20015;
		 public const ushort ObjectRemoveResponse = 20016;
		 public const ushort ObjectGetRequest = 20017;
		 public const ushort ObjectGetResponse = 20018;
		 public const ushort R2G_GetLoginKey = 20019;
		 public const ushort G2R_GetLoginKey = 20020;
		 public const ushort G2M_SessionDisconnect = 20021;
		 public const ushort ObjectQueryResponse = 20022;
		 public const ushort M2M_UnitTransferRequest = 20023;
		 public const ushort M2M_UnitTransferResponse = 20024;
	}
}
