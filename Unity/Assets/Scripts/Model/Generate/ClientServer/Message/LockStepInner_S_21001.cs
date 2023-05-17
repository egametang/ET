using ET;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
// 请求匹配
	[ResponseType(nameof(Match2G_Match))]
	[Message(LockStepInner.G2Match_Match)]
	[MemoryPackable]
	public partial class G2Match_Match: MessageObject, IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

	}

	[Message(LockStepInner.Match2G_Match)]
	[MemoryPackable]
	public partial class Match2G_Match: MessageObject, IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(Map2Match_GetRoom))]
	[Message(LockStepInner.Match2Map_GetRoom)]
	[MemoryPackable]
	public partial class Match2Map_GetRoom: MessageObject, IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public List<long> PlayerIds { get; set; }

	}

	[Message(LockStepInner.Map2Match_GetRoom)]
	[MemoryPackable]
	public partial class Map2Match_GetRoom: MessageObject, IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

// 房间的instanceId
		[MemoryPackOrder(3)]
		public long InstanceId { get; set; }

	}

	[ResponseType(nameof(Room2G_Reconnect))]
	[Message(LockStepInner.G2Room_Reconnect)]
	[MemoryPackable]
	public partial class G2Room_Reconnect: MessageObject, IActorRequest
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long PlayerId { get; set; }

	}

	[Message(LockStepInner.Room2G_Reconnect)]
	[MemoryPackable]
	public partial class Room2G_Reconnect: MessageObject, IActorResponse
	{
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long StartTime { get; set; }

		[MemoryPackOrder(4)]
		public List<LockStepUnitInfo> UnitInfos { get; set; }

		[MemoryPackOrder(5)]
		public int Frame { get; set; }

	}

	public static class LockStepInner
	{
		 public const ushort G2Match_Match = 21002;
		 public const ushort Match2G_Match = 21003;
		 public const ushort Match2Map_GetRoom = 21004;
		 public const ushort Map2Match_GetRoom = 21005;
		 public const ushort G2Room_Reconnect = 21006;
		 public const ushort Room2G_Reconnect = 21007;
	}
}
