using ET;
using ProtoBuf;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
// using
	[ResponseType(nameof(ObjectQueryResponse))]
	[Message(InnerMessage.ObjectQueryRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectQueryRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public long InstanceId { get; set; }

	}

	[ResponseType(nameof(A2M_Reload))]
	[Message(InnerMessage.M2A_Reload)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2A_Reload: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.A2M_Reload)]
	[ProtoContract]
	[MemoryPackable]
	public partial class A2M_Reload: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2G_LockResponse))]
	[Message(InnerMessage.G2G_LockRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2G_LockRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Address { get; set; }

	}

	[Message(InnerMessage.G2G_LockResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2G_LockResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2G_LockReleaseResponse))]
	[Message(InnerMessage.G2G_LockReleaseRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2G_LockReleaseRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Address { get; set; }

	}

	[Message(InnerMessage.G2G_LockReleaseResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2G_LockReleaseResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectAddResponse))]
	[Message(InnerMessage.ObjectAddRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectAddRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public long Key { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public long InstanceId { get; set; }

	}

	[Message(InnerMessage.ObjectAddResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectAddResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectLockResponse))]
	[Message(InnerMessage.ObjectLockRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectLockRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public long Key { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public long InstanceId { get; set; }

		[ProtoMember(5)]
		[MemoryPackOrder(4)]
		public int Time { get; set; }

	}

	[Message(InnerMessage.ObjectLockResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectLockResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectUnLockResponse))]
	[Message(InnerMessage.ObjectUnLockRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectUnLockRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public long Key { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public long OldInstanceId { get; set; }

		[ProtoMember(5)]
		[MemoryPackOrder(4)]
		public long InstanceId { get; set; }

	}

	[Message(InnerMessage.ObjectUnLockResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectUnLockResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectRemoveResponse))]
	[Message(InnerMessage.ObjectRemoveRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectRemoveRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public long Key { get; set; }

	}

	[Message(InnerMessage.ObjectRemoveResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectRemoveResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(ObjectGetResponse))]
	[Message(InnerMessage.ObjectGetRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectGetRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Type { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public long Key { get; set; }

	}

	[Message(InnerMessage.ObjectGetResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectGetResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public int Type { get; set; }

		[ProtoMember(5)]
		[MemoryPackOrder(4)]
		public long InstanceId { get; set; }

	}

	[ResponseType(nameof(G2R_GetLoginKey))]
	[Message(InnerMessage.R2G_GetLoginKey)]
	[ProtoContract]
	[MemoryPackable]
	public partial class R2G_GetLoginKey: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public string Account { get; set; }

	}

	[Message(InnerMessage.G2R_GetLoginKey)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2R_GetLoginKey: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public long Key { get; set; }

		[ProtoMember(5)]
		[MemoryPackOrder(4)]
		public long GateId { get; set; }

	}

	[Message(InnerMessage.G2M_SessionDisconnect)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2M_SessionDisconnect: ProtoObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.ObjectQueryResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class ObjectQueryResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public byte[] Entity { get; set; }

	}

	[ResponseType(nameof(M2M_UnitTransferResponse))]
	[Message(InnerMessage.M2M_UnitTransferRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2M_UnitTransferRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long OldInstanceId { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public byte[] Unit { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public List<byte[]> Entitys { get; set; }

	}

	[Message(InnerMessage.M2M_UnitTransferResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2M_UnitTransferResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[ProtoMember(3)]
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
