using ET;
using ProtoBuf;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
	[ResponseType(nameof(G2C_Match))]
	[Message(LockStepOuter.C2G_Match)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2G_Match: MessageObject, IRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(LockStepOuter.G2C_Match)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2C_Match: MessageObject, IResponse
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

// 匹配成功，通知客户端切换场景
	[Message(LockStepOuter.Match2G_NotifyMatchSuccess)]
	[ProtoContract]
	[MemoryPackable]
	public partial class Match2G_NotifyMatchSuccess: MessageObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

// 房间的instanceId
		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long InstanceId { get; set; }

	}

// 客户端通知房间切换场景完成
	[Message(LockStepOuter.C2Room_ChangeSceneFinish)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2Room_ChangeSceneFinish: MessageObject, IActorRoom
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public long PlayerId { get; set; }

	}

	[Message(LockStepOuter.LockStepUnitInfo)]
	[ProtoContract]
	[MemoryPackable]
	public partial class LockStepUnitInfo: MessageObject
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public long PlayerId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public TrueSync.TSVector Position { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public TrueSync.TSQuaternion Rotation { get; set; }

	}

// 房间通知客户端进入战斗
	[Message(LockStepOuter.Room2C_Start)]
	[ProtoContract]
	[MemoryPackable]
	public partial class Room2C_Start: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public long StartTime { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public List<LockStepUnitInfo> UnitInfo { get; set; }

	}

	[Message(LockStepOuter.LSInput)]
	[ProtoContract]
	[MemoryPackable]
	public partial class LSInput: MessageObject
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public TrueSync.TSVector2 V { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int Button { get; set; }

	}

	[Message(LockStepOuter.FrameMessage)]
	[ProtoContract]
	[MemoryPackable]
	public partial class FrameMessage: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int Frame { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long PlayerId { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public LSInput Input { get; set; }

	}

	[Message(LockStepOuter.OneFrameMessages)]
	[ProtoContract]
	[MemoryPackable]
	public partial class OneFrameMessages: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int Frame { get; set; }

		[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public Dictionary<long, LSInput> Inputs { get; set; }
	}

	[Message(LockStepOuter.Room2C_AdjustUpdateTime)]
	[ProtoContract]
	[MemoryPackable]
	public partial class Room2C_AdjustUpdateTime: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int DiffTime { get; set; }

	}

	public static class LockStepOuter
	{
		 public const ushort C2G_Match = 11002;
		 public const ushort G2C_Match = 11003;
		 public const ushort Match2G_NotifyMatchSuccess = 11004;
		 public const ushort C2Room_ChangeSceneFinish = 11005;
		 public const ushort LockStepUnitInfo = 11006;
		 public const ushort Room2C_Start = 11007;
		 public const ushort LSInput = 11008;
		 public const ushort FrameMessage = 11009;
		 public const ushort OneFrameMessages = 11010;
		 public const ushort Room2C_AdjustUpdateTime = 11011;
	}
}
