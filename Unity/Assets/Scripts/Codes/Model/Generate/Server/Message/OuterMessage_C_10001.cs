using ET;
using ProtoBuf;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
	[Message(OuterMessage.HttpGetRouterResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class HttpGetRouterResponse: MessageObject
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public List<string> Realms { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public List<string> Routers { get; set; }

	}

	[Message(OuterMessage.RouterSync)]
	[ProtoContract]
	[MemoryPackable]
	public partial class RouterSync: MessageObject
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public uint ConnectId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public string Address { get; set; }

	}

	[ResponseType(nameof(M2C_TestResponse))]
	[Message(OuterMessage.C2M_TestRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2M_TestRequest: MessageObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public string request { get; set; }

	}

	[Message(OuterMessage.M2C_TestResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_TestResponse: MessageObject, IActorResponse
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
		public string response { get; set; }

	}

	[ResponseType(nameof(Actor_TransferResponse))]
	[Message(OuterMessage.Actor_TransferRequest)]
	[ProtoContract]
	[MemoryPackable]
	public partial class Actor_TransferRequest: MessageObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int MapIndex { get; set; }

	}

	[Message(OuterMessage.Actor_TransferResponse)]
	[ProtoContract]
	[MemoryPackable]
	public partial class Actor_TransferResponse: MessageObject, IActorLocationResponse
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

	[ResponseType(nameof(G2C_EnterMap))]
	[Message(OuterMessage.C2G_EnterMap)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2G_EnterMap: MessageObject, IRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_EnterMap)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2C_EnterMap: MessageObject, IResponse
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

// 自己unitId
		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public long MyId { get; set; }

	}

	[Message(OuterMessage.MoveInfo)]
	[ProtoContract]
	[MemoryPackable]
	public partial class MoveInfo: MessageObject
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public List<Unity.Mathematics.float3> Points { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public Unity.Mathematics.quaternion Rotation { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public int TurnSpeed { get; set; }

	}

	[Message(OuterMessage.UnitInfo)]
	[ProtoContract]
	[MemoryPackable]
	public partial class UnitInfo: MessageObject
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public long UnitId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int ConfigId { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public int Type { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public Unity.Mathematics.float3 Position { get; set; }

		[ProtoMember(5)]
		[MemoryPackOrder(4)]
		public Unity.Mathematics.float3 Forward { get; set; }

		[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
		[ProtoMember(6)]
		[MemoryPackOrder(5)]
		public Dictionary<int, long> KV { get; set; }
		[ProtoMember(7)]
		[MemoryPackOrder(6)]
		public MoveInfo MoveInfo { get; set; }

	}

	[Message(OuterMessage.M2C_CreateUnits)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_CreateUnits: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public List<UnitInfo> Units { get; set; }

	}

	[Message(OuterMessage.M2C_CreateMyUnit)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_CreateMyUnit: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public UnitInfo Unit { get; set; }

	}

	[Message(OuterMessage.M2C_StartSceneChange)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_StartSceneChange: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public long SceneInstanceId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public string SceneName { get; set; }

	}

	[Message(OuterMessage.M2C_RemoveUnits)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_RemoveUnits: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public List<long> Units { get; set; }

	}

	[Message(OuterMessage.C2M_PathfindingResult)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2M_PathfindingResult: MessageObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public Unity.Mathematics.float3 Position { get; set; }

	}

	[Message(OuterMessage.C2M_Stop)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2M_Stop: MessageObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.M2C_PathfindingResult)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_PathfindingResult: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public long Id { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public Unity.Mathematics.float3 Position { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public List<Unity.Mathematics.float3> Points { get; set; }

	}

	[Message(OuterMessage.M2C_Stop)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_Stop: MessageObject, IActorMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int Error { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public Unity.Mathematics.float3 Position { get; set; }

		[ProtoMember(4)]
		[MemoryPackOrder(3)]
		public Unity.Mathematics.quaternion Rotation { get; set; }

	}

	[ResponseType(nameof(G2C_Ping))]
	[Message(OuterMessage.C2G_Ping)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2G_Ping: MessageObject, IRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Ping)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2C_Ping: MessageObject, IResponse
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
		public long Time { get; set; }

	}

	[Message(OuterMessage.G2C_Test)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2C_Test: MessageObject, IMessage
	{
	}

	[ResponseType(nameof(M2C_Reload))]
	[Message(OuterMessage.C2M_Reload)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2M_Reload: MessageObject, IRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public string Account { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.M2C_Reload)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_Reload: MessageObject, IResponse
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

	[ResponseType(nameof(R2C_Login))]
	[Message(OuterMessage.C2R_Login)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2R_Login: MessageObject, IRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public string Account { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.R2C_Login)]
	[ProtoContract]
	[MemoryPackable]
	public partial class R2C_Login: MessageObject, IResponse
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
		public string Address { get; set; }

		[ProtoMember(5)]
		[MemoryPackOrder(4)]
		public long Key { get; set; }

		[ProtoMember(6)]
		[MemoryPackOrder(5)]
		public long GateId { get; set; }

	}

	[ResponseType(nameof(G2C_LoginGate))]
	[Message(OuterMessage.C2G_LoginGate)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2G_LoginGate: MessageObject, IRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[ProtoMember(3)]
		[MemoryPackOrder(2)]
		public long GateId { get; set; }

	}

	[Message(OuterMessage.G2C_LoginGate)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2C_LoginGate: MessageObject, IResponse
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
		public long PlayerId { get; set; }

	}

	[Message(OuterMessage.G2C_TestHotfixMessage)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2C_TestHotfixMessage: MessageObject, IMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public string Info { get; set; }

	}

	[ResponseType(nameof(M2C_TestRobotCase))]
	[Message(OuterMessage.C2M_TestRobotCase)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2M_TestRobotCase: MessageObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int N { get; set; }

	}

	[Message(OuterMessage.M2C_TestRobotCase)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_TestRobotCase: MessageObject, IActorLocationResponse
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
		public int N { get; set; }

	}

	[Message(OuterMessage.C2M_TestRobotCase2)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2M_TestRobotCase2: MessageObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int N { get; set; }

	}

	[Message(OuterMessage.M2C_TestRobotCase2)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_TestRobotCase2: MessageObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		[MemoryPackOrder(1)]
		public int N { get; set; }

	}

	[ResponseType(nameof(M2C_TransferMap))]
	[Message(OuterMessage.C2M_TransferMap)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2M_TransferMap: MessageObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.M2C_TransferMap)]
	[ProtoContract]
	[MemoryPackable]
	public partial class M2C_TransferMap: MessageObject, IActorLocationResponse
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

	[ResponseType(nameof(G2C_Benchmark))]
	[Message(OuterMessage.C2G_Benchmark)]
	[ProtoContract]
	[MemoryPackable]
	public partial class C2G_Benchmark: MessageObject, IRequest
	{
		[ProtoMember(1)]
		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Benchmark)]
	[ProtoContract]
	[MemoryPackable]
	public partial class G2C_Benchmark: MessageObject, IResponse
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

	public static class OuterMessage
	{
		 public const ushort HttpGetRouterResponse = 10002;
		 public const ushort RouterSync = 10003;
		 public const ushort C2M_TestRequest = 10004;
		 public const ushort M2C_TestResponse = 10005;
		 public const ushort Actor_TransferRequest = 10006;
		 public const ushort Actor_TransferResponse = 10007;
		 public const ushort C2G_EnterMap = 10008;
		 public const ushort G2C_EnterMap = 10009;
		 public const ushort MoveInfo = 10010;
		 public const ushort UnitInfo = 10011;
		 public const ushort M2C_CreateUnits = 10012;
		 public const ushort M2C_CreateMyUnit = 10013;
		 public const ushort M2C_StartSceneChange = 10014;
		 public const ushort M2C_RemoveUnits = 10015;
		 public const ushort C2M_PathfindingResult = 10016;
		 public const ushort C2M_Stop = 10017;
		 public const ushort M2C_PathfindingResult = 10018;
		 public const ushort M2C_Stop = 10019;
		 public const ushort C2G_Ping = 10020;
		 public const ushort G2C_Ping = 10021;
		 public const ushort G2C_Test = 10022;
		 public const ushort C2M_Reload = 10023;
		 public const ushort M2C_Reload = 10024;
		 public const ushort C2R_Login = 10025;
		 public const ushort R2C_Login = 10026;
		 public const ushort C2G_LoginGate = 10027;
		 public const ushort G2C_LoginGate = 10028;
		 public const ushort G2C_TestHotfixMessage = 10029;
		 public const ushort C2M_TestRobotCase = 10030;
		 public const ushort M2C_TestRobotCase = 10031;
		 public const ushort C2M_TestRobotCase2 = 10032;
		 public const ushort M2C_TestRobotCase2 = 10033;
		 public const ushort C2M_TransferMap = 10034;
		 public const ushort M2C_TransferMap = 10035;
		 public const ushort C2G_Benchmark = 10036;
		 public const ushort G2C_Benchmark = 10037;
	}
}
