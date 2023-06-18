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
		public static G2Match_Match Create(bool isFromPool = false) { return !isFromPool? new G2Match_Match() : NetServices.Instance.FetchMessage(typeof(G2Match_Match)) as G2Match_Match; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

	}

	[Message(LockStepInner.Match2G_Match)]
	[MemoryPackable]
	public partial class Match2G_Match: MessageObject, IActorResponse
	{
		public static Match2G_Match Create(bool isFromPool = false) { return !isFromPool? new Match2G_Match() : NetServices.Instance.FetchMessage(typeof(Match2G_Match)) as Match2G_Match; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

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
		public static Match2Map_GetRoom Create(bool isFromPool = false) { return !isFromPool? new Match2Map_GetRoom() : NetServices.Instance.FetchMessage(typeof(Match2Map_GetRoom)) as Match2Map_GetRoom; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public List<long> PlayerIds { get; set; }

	}

	[Message(LockStepInner.Map2Match_GetRoom)]
	[MemoryPackable]
	public partial class Map2Match_GetRoom: MessageObject, IActorResponse
	{
		public static Map2Match_GetRoom Create(bool isFromPool = false) { return !isFromPool? new Map2Match_GetRoom() : NetServices.Instance.FetchMessage(typeof(Map2Match_GetRoom)) as Map2Match_GetRoom; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

// 房间的ActorId
		[MemoryPackOrder(3)]
		public ActorId ActorId { get; set; }

	}

	[ResponseType(nameof(Room2G_Reconnect))]
	[Message(LockStepInner.G2Room_Reconnect)]
	[MemoryPackable]
	public partial class G2Room_Reconnect: MessageObject, IActorRequest
	{
		public static G2Room_Reconnect Create(bool isFromPool = false) { return !isFromPool? new G2Room_Reconnect() : NetServices.Instance.FetchMessage(typeof(G2Room_Reconnect)) as G2Room_Reconnect; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long PlayerId { get; set; }

	}

	[Message(LockStepInner.Room2G_Reconnect)]
	[MemoryPackable]
	public partial class Room2G_Reconnect: MessageObject, IActorResponse
	{
		public static Room2G_Reconnect Create(bool isFromPool = false) { return !isFromPool? new Room2G_Reconnect() : NetServices.Instance.FetchMessage(typeof(Room2G_Reconnect)) as Room2G_Reconnect; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

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
