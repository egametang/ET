using ET;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
	[ResponseType(nameof(G2C_Match))]
	[Message(LockStepOuter.C2G_Match)]
	[MemoryPackable]
	public partial class C2G_Match: MessageObject, IRequest
	{
		public static C2G_Match Create(bool isFromPool = false) { return !isFromPool? new C2G_Match() : NetServices.Instance.FetchMessage(typeof(C2G_Match)) as C2G_Match; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(LockStepOuter.G2C_Match)]
	[MemoryPackable]
	public partial class G2C_Match: MessageObject, IResponse
	{
		public static G2C_Match Create(bool isFromPool = false) { return !isFromPool? new G2C_Match() : NetServices.Instance.FetchMessage(typeof(G2C_Match)) as G2C_Match; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

// 匹配成功，通知客户端切换场景
	[Message(LockStepOuter.Match2G_NotifyMatchSuccess)]
	[MemoryPackable]
	public partial class Match2G_NotifyMatchSuccess: MessageObject, IActorMessage
	{
		public static Match2G_NotifyMatchSuccess Create(bool isFromPool = false) { return !isFromPool? new Match2G_NotifyMatchSuccess() : NetServices.Instance.FetchMessage(typeof(Match2G_NotifyMatchSuccess)) as Match2G_NotifyMatchSuccess; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

// 房间的ActorId
		[MemoryPackOrder(1)]
		public ActorId ActorId { get; set; }

	}

// 客户端通知房间切换场景完成
	[Message(LockStepOuter.C2Room_ChangeSceneFinish)]
	[MemoryPackable]
	public partial class C2Room_ChangeSceneFinish: MessageObject, IActorRoom
	{
		public static C2Room_ChangeSceneFinish Create(bool isFromPool = false) { return !isFromPool? new C2Room_ChangeSceneFinish() : NetServices.Instance.FetchMessage(typeof(C2Room_ChangeSceneFinish)) as C2Room_ChangeSceneFinish; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long PlayerId { get; set; }

	}

	[Message(LockStepOuter.LockStepUnitInfo)]
	[MemoryPackable]
	public partial class LockStepUnitInfo: MessageObject
	{
		public static LockStepUnitInfo Create(bool isFromPool = false) { return !isFromPool? new LockStepUnitInfo() : NetServices.Instance.FetchMessage(typeof(LockStepUnitInfo)) as LockStepUnitInfo; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long PlayerId { get; set; }

		[MemoryPackOrder(1)]
		public TrueSync.TSVector Position { get; set; }

		[MemoryPackOrder(2)]
		public TrueSync.TSQuaternion Rotation { get; set; }

	}

// 房间通知客户端进入战斗
	[Message(LockStepOuter.Room2C_Start)]
	[MemoryPackable]
	public partial class Room2C_Start: MessageObject, IActorMessage
	{
		public static Room2C_Start Create(bool isFromPool = false) { return !isFromPool? new Room2C_Start() : NetServices.Instance.FetchMessage(typeof(Room2C_Start)) as Room2C_Start; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long StartTime { get; set; }

		[MemoryPackOrder(1)]
		public List<LockStepUnitInfo> UnitInfo { get; set; }

	}

	[Message(LockStepOuter.FrameMessage)]
	[MemoryPackable]
	public partial class FrameMessage: MessageObject, IActorMessage
	{
		public static FrameMessage Create(bool isFromPool = false) { return !isFromPool? new FrameMessage() : NetServices.Instance.FetchMessage(typeof(FrameMessage)) as FrameMessage; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int Frame { get; set; }

		[MemoryPackOrder(1)]
		public long PlayerId { get; set; }

		[MemoryPackOrder(2)]
		public LSInput Input { get; set; }

	}

	[Message(LockStepOuter.OneFrameInputs)]
	[MemoryPackable]
	public partial class OneFrameInputs: MessageObject, IActorMessage
	{
		public static OneFrameInputs Create(bool isFromPool = false) { return !isFromPool? new OneFrameInputs() : NetServices.Instance.FetchMessage(typeof(OneFrameInputs)) as OneFrameInputs; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
		[MemoryPackOrder(1)]
		public Dictionary<long, LSInput> Inputs { get; set; }
	}

	[Message(LockStepOuter.Room2C_AdjustUpdateTime)]
	[MemoryPackable]
	public partial class Room2C_AdjustUpdateTime: MessageObject, IActorMessage
	{
		public static Room2C_AdjustUpdateTime Create(bool isFromPool = false) { return !isFromPool? new Room2C_AdjustUpdateTime() : NetServices.Instance.FetchMessage(typeof(Room2C_AdjustUpdateTime)) as Room2C_AdjustUpdateTime; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int DiffTime { get; set; }

	}

	[Message(LockStepOuter.C2Room_CheckHash)]
	[MemoryPackable]
	public partial class C2Room_CheckHash: MessageObject, IActorRoom
	{
		public static C2Room_CheckHash Create(bool isFromPool = false) { return !isFromPool? new C2Room_CheckHash() : NetServices.Instance.FetchMessage(typeof(C2Room_CheckHash)) as C2Room_CheckHash; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long PlayerId { get; set; }

		[MemoryPackOrder(1)]
		public int Frame { get; set; }

		[MemoryPackOrder(2)]
		public long Hash { get; set; }

	}

	[Message(LockStepOuter.Room2C_CheckHashFail)]
	[MemoryPackable]
	public partial class Room2C_CheckHashFail: MessageObject, IActorMessage
	{
		public static Room2C_CheckHashFail Create(bool isFromPool = false) { return !isFromPool? new Room2C_CheckHashFail() : NetServices.Instance.FetchMessage(typeof(Room2C_CheckHashFail)) as Room2C_CheckHashFail; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int Frame { get; set; }

		[MemoryPackOrder(1)]
		public byte[] LSWorldBytes { get; set; }

	}

	[Message(LockStepOuter.G2C_Reconnect)]
	[MemoryPackable]
	public partial class G2C_Reconnect: MessageObject, IActorMessage
	{
		public static G2C_Reconnect Create(bool isFromPool = false) { return !isFromPool? new G2C_Reconnect() : NetServices.Instance.FetchMessage(typeof(G2C_Reconnect)) as G2C_Reconnect; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long StartTime { get; set; }

		[MemoryPackOrder(1)]
		public List<LockStepUnitInfo> UnitInfos { get; set; }

		[MemoryPackOrder(2)]
		public int Frame { get; set; }

	}

	public static class LockStepOuter
	{
		 public const ushort C2G_Match = 11002;
		 public const ushort G2C_Match = 11003;
		 public const ushort Match2G_NotifyMatchSuccess = 11004;
		 public const ushort C2Room_ChangeSceneFinish = 11005;
		 public const ushort LockStepUnitInfo = 11006;
		 public const ushort Room2C_Start = 11007;
		 public const ushort FrameMessage = 11008;
		 public const ushort OneFrameInputs = 11009;
		 public const ushort Room2C_AdjustUpdateTime = 11010;
		 public const ushort C2Room_CheckHash = 11011;
		 public const ushort Room2C_CheckHashFail = 11012;
		 public const ushort G2C_Reconnect = 11013;
	}
}
