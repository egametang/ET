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
		public static ObjectQueryRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectQueryRequest() : ObjectPool.Instance.Fetch(typeof(ObjectQueryRequest)) as ObjectQueryRequest; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[MemoryPackOrder(2)]
		public long InstanceId { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Key = default;
			this.InstanceId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(A2M_Reload))]
	[Message(InnerMessage.M2A_Reload)]
	[MemoryPackable]
	public partial class M2A_Reload: MessageObject, IActorRequest
	{
		public static M2A_Reload Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new M2A_Reload() : ObjectPool.Instance.Fetch(typeof(M2A_Reload)) as M2A_Reload; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.A2M_Reload)]
	[MemoryPackable]
	public partial class A2M_Reload: MessageObject, IActorResponse
	{
		public static A2M_Reload Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new A2M_Reload() : ObjectPool.Instance.Fetch(typeof(A2M_Reload)) as A2M_Reload; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(G2G_LockResponse))]
	[Message(InnerMessage.G2G_LockRequest)]
	[MemoryPackable]
	public partial class G2G_LockRequest: MessageObject, IActorRequest
	{
		public static G2G_LockRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new G2G_LockRequest() : ObjectPool.Instance.Fetch(typeof(G2G_LockRequest)) as G2G_LockRequest; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[MemoryPackOrder(2)]
		public string Address { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Id = default;
			this.Address = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.G2G_LockResponse)]
	[MemoryPackable]
	public partial class G2G_LockResponse: MessageObject, IActorResponse
	{
		public static G2G_LockResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new G2G_LockResponse() : ObjectPool.Instance.Fetch(typeof(G2G_LockResponse)) as G2G_LockResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(G2G_LockReleaseResponse))]
	[Message(InnerMessage.G2G_LockReleaseRequest)]
	[MemoryPackable]
	public partial class G2G_LockReleaseRequest: MessageObject, IActorRequest
	{
		public static G2G_LockReleaseRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new G2G_LockReleaseRequest() : ObjectPool.Instance.Fetch(typeof(G2G_LockReleaseRequest)) as G2G_LockReleaseRequest; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[MemoryPackOrder(2)]
		public string Address { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Id = default;
			this.Address = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.G2G_LockReleaseResponse)]
	[MemoryPackable]
	public partial class G2G_LockReleaseResponse: MessageObject, IActorResponse
	{
		public static G2G_LockReleaseResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new G2G_LockReleaseResponse() : ObjectPool.Instance.Fetch(typeof(G2G_LockReleaseResponse)) as G2G_LockReleaseResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(ObjectAddResponse))]
	[Message(InnerMessage.ObjectAddRequest)]
	[MemoryPackable]
	public partial class ObjectAddRequest: MessageObject, IActorRequest
	{
		public static ObjectAddRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectAddRequest() : ObjectPool.Instance.Fetch(typeof(ObjectAddRequest)) as ObjectAddRequest; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

		[MemoryPackOrder(3)]
		public ActorId ActorId { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Type = default;
			this.Key = default;
			this.ActorId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.ObjectAddResponse)]
	[MemoryPackable]
	public partial class ObjectAddResponse: MessageObject, IActorResponse
	{
		public static ObjectAddResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectAddResponse() : ObjectPool.Instance.Fetch(typeof(ObjectAddResponse)) as ObjectAddResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(ObjectLockResponse))]
	[Message(InnerMessage.ObjectLockRequest)]
	[MemoryPackable]
	public partial class ObjectLockRequest: MessageObject, IActorRequest
	{
		public static ObjectLockRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectLockRequest() : ObjectPool.Instance.Fetch(typeof(ObjectLockRequest)) as ObjectLockRequest; 
		}

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

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Type = default;
			this.Key = default;
			this.ActorId = default;
			this.Time = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.ObjectLockResponse)]
	[MemoryPackable]
	public partial class ObjectLockResponse: MessageObject, IActorResponse
	{
		public static ObjectLockResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectLockResponse() : ObjectPool.Instance.Fetch(typeof(ObjectLockResponse)) as ObjectLockResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(ObjectUnLockResponse))]
	[Message(InnerMessage.ObjectUnLockRequest)]
	[MemoryPackable]
	public partial class ObjectUnLockRequest: MessageObject, IActorRequest
	{
		public static ObjectUnLockRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectUnLockRequest() : ObjectPool.Instance.Fetch(typeof(ObjectUnLockRequest)) as ObjectUnLockRequest; 
		}

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

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Type = default;
			this.Key = default;
			this.OldActorId = default;
			this.NewActorId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.ObjectUnLockResponse)]
	[MemoryPackable]
	public partial class ObjectUnLockResponse: MessageObject, IActorResponse
	{
		public static ObjectUnLockResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectUnLockResponse() : ObjectPool.Instance.Fetch(typeof(ObjectUnLockResponse)) as ObjectUnLockResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(ObjectRemoveResponse))]
	[Message(InnerMessage.ObjectRemoveRequest)]
	[MemoryPackable]
	public partial class ObjectRemoveRequest: MessageObject, IActorRequest
	{
		public static ObjectRemoveRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectRemoveRequest() : ObjectPool.Instance.Fetch(typeof(ObjectRemoveRequest)) as ObjectRemoveRequest; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Type = default;
			this.Key = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.ObjectRemoveResponse)]
	[MemoryPackable]
	public partial class ObjectRemoveResponse: MessageObject, IActorResponse
	{
		public static ObjectRemoveResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectRemoveResponse() : ObjectPool.Instance.Fetch(typeof(ObjectRemoveResponse)) as ObjectRemoveResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(ObjectGetResponse))]
	[Message(InnerMessage.ObjectGetRequest)]
	[MemoryPackable]
	public partial class ObjectGetRequest: MessageObject, IActorRequest
	{
		public static ObjectGetRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectGetRequest() : ObjectPool.Instance.Fetch(typeof(ObjectGetRequest)) as ObjectGetRequest; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[MemoryPackOrder(2)]
		public long Key { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Type = default;
			this.Key = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.ObjectGetResponse)]
	[MemoryPackable]
	public partial class ObjectGetResponse: MessageObject, IActorResponse
	{
		public static ObjectGetResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectGetResponse() : ObjectPool.Instance.Fetch(typeof(ObjectGetResponse)) as ObjectGetResponse; 
		}

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

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			this.Type = default;
			this.ActorId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(G2R_GetLoginKey))]
	[Message(InnerMessage.R2G_GetLoginKey)]
	[MemoryPackable]
	public partial class R2G_GetLoginKey: MessageObject, IActorRequest
	{
		public static R2G_GetLoginKey Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new R2G_GetLoginKey() : ObjectPool.Instance.Fetch(typeof(R2G_GetLoginKey)) as R2G_GetLoginKey; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Account { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Account = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.G2R_GetLoginKey)]
	[MemoryPackable]
	public partial class G2R_GetLoginKey: MessageObject, IActorResponse
	{
		public static G2R_GetLoginKey Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new G2R_GetLoginKey() : ObjectPool.Instance.Fetch(typeof(G2R_GetLoginKey)) as G2R_GetLoginKey; 
		}

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

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			this.Key = default;
			this.GateId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.G2M_SessionDisconnect)]
	[MemoryPackable]
	public partial class G2M_SessionDisconnect: MessageObject, IActorLocationMessage
	{
		public static G2M_SessionDisconnect Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new G2M_SessionDisconnect() : ObjectPool.Instance.Fetch(typeof(G2M_SessionDisconnect)) as G2M_SessionDisconnect; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.ObjectQueryResponse)]
	[MemoryPackable]
	public partial class ObjectQueryResponse: MessageObject, IActorResponse
	{
		public static ObjectQueryResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new ObjectQueryResponse() : ObjectPool.Instance.Fetch(typeof(ObjectQueryResponse)) as ObjectQueryResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public byte[] Entity { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			this.Entity = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[ResponseType(nameof(M2M_UnitTransferResponse))]
	[Message(InnerMessage.M2M_UnitTransferRequest)]
	[MemoryPackable]
	public partial class M2M_UnitTransferRequest: MessageObject, IActorRequest
	{
		public static M2M_UnitTransferRequest Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new M2M_UnitTransferRequest() : ObjectPool.Instance.Fetch(typeof(M2M_UnitTransferRequest)) as M2M_UnitTransferRequest; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public ActorId OldActorId { get; set; }

		[MemoryPackOrder(2)]
		public byte[] Unit { get; set; }

		[MemoryPackOrder(3)]
		public List<byte[]> Entitys { get; set; } = new();

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.OldActorId = default;
			this.Unit = default;
			this.Entitys.Clear();
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(InnerMessage.M2M_UnitTransferResponse)]
	[MemoryPackable]
	public partial class M2M_UnitTransferResponse: MessageObject, IActorResponse
	{
		public static M2M_UnitTransferResponse Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new M2M_UnitTransferResponse() : ObjectPool.Instance.Fetch(typeof(M2M_UnitTransferResponse)) as M2M_UnitTransferResponse; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

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
