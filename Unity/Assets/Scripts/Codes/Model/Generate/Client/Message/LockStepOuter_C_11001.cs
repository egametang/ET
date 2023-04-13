using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
	[ResponseType(nameof(G2C_Match))]
	[Message(LockStepOuter.C2G_Match)]
	[ProtoContract]
	public partial class C2G_Match: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(LockStepOuter.G2C_Match)]
	[ProtoContract]
	public partial class G2C_Match: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

// 匹配成功，通知客户端切换场景
	[Message(LockStepOuter.Match2G_NotifyMatchSuccess)]
	[ProtoContract]
	public partial class Match2G_NotifyMatchSuccess: ProtoObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

// 房间的instanceId
		[ProtoMember(2)]
		public long InstanceId { get; set; }

	}

// 客户端通知房间切换场景完成
	[Message(LockStepOuter.C2Room_ChangeSceneFinish)]
	[ProtoContract]
	public partial class C2Room_ChangeSceneFinish: ProtoObject, IActorRoom
	{
		[ProtoMember(1)]
		public long PlayerId { get; set; }

	}

	[Message(LockStepOuter.LockStepUnitInfo)]
	[ProtoContract]
	public partial class LockStepUnitInfo: ProtoObject
	{
		[ProtoMember(1)]
		public long PlayerId { get; set; }

		[ProtoMember(2)]
		public TrueSync.TSVector Position { get; set; }

		[ProtoMember(3)]
		public TrueSync.TSQuaternion Rotation { get; set; }

	}

// 房间通知客户端进入战斗
	[Message(LockStepOuter.Room2C_EnterMap)]
	[ProtoContract]
	public partial class Room2C_EnterMap: ProtoObject, IActorMessage
	{
		[ProtoMember(2)]
		public List<LockStepUnitInfo> UnitInfo { get; set; }

	}

	public static class LockStepOuter
	{
		 public const ushort C2G_Match = 11002;
		 public const ushort G2C_Match = 11003;
		 public const ushort Match2G_NotifyMatchSuccess = 11004;
		 public const ushort C2Room_ChangeSceneFinish = 11005;
		 public const ushort LockStepUnitInfo = 11006;
		 public const ushort Room2C_EnterMap = 11007;
	}
}
