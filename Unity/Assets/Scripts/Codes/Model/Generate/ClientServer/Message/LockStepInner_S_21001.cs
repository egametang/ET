using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
// 请求匹配
	[ResponseType(nameof(Match2G_Match))]
	[Message(LockStepInner.G2Match_Match)]
	[ProtoContract]
	public partial class G2Match_Match: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Id { get; set; }

	}

	[Message(LockStepInner.Match2G_Match)]
	[ProtoContract]
	public partial class Match2G_Match: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(Map2Match_GetRoom))]
	[Message(LockStepInner.Match2Map_GetRoom)]
	[ProtoContract]
	public partial class Match2Map_GetRoom: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public List<long> PlayerIds { get; set; }

	}

	[Message(LockStepInner.Map2Match_GetRoom)]
	[ProtoContract]
	public partial class Map2Match_GetRoom: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

// 房间的instanceId
		[ProtoMember(4)]
		public long InstanceId { get; set; }

	}

	public static class LockStepInner
	{
		 public const ushort G2Match_Match = 21002;
		 public const ushort Match2G_Match = 21003;
		 public const ushort Match2Map_GetRoom = 21004;
		 public const ushort Map2Match_GetRoom = 21005;
	}
}
