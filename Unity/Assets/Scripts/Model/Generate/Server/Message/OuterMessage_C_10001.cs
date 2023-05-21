using ET;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
	[Message(OuterMessage.HttpGetRouterResponse)]
	[MemoryPackable]
	public partial class HttpGetRouterResponse: MessageObject
	{
		public static HttpGetRouterResponse Create(bool isFromPool = false) { return !isFromPool? new HttpGetRouterResponse() : NetServices.Instance.FetchMessage(typeof(HttpGetRouterResponse)) as HttpGetRouterResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public List<string> Realms { get; set; }

		[MemoryPackOrder(1)]
		public List<string> Routers { get; set; }

	}

	[Message(OuterMessage.RouterSync)]
	[MemoryPackable]
	public partial class RouterSync: MessageObject
	{
		public static RouterSync Create(bool isFromPool = false) { return !isFromPool? new RouterSync() : NetServices.Instance.FetchMessage(typeof(RouterSync)) as RouterSync; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public uint ConnectId { get; set; }

		[MemoryPackOrder(1)]
		public string Address { get; set; }

	}

	[ResponseType(nameof(M2C_TestResponse))]
	[Message(OuterMessage.C2M_TestRequest)]
	[MemoryPackable]
	public partial class C2M_TestRequest: MessageObject, IActorLocationRequest
	{
		public static C2M_TestRequest Create(bool isFromPool = false) { return !isFromPool? new C2M_TestRequest() : NetServices.Instance.FetchMessage(typeof(C2M_TestRequest)) as C2M_TestRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string request { get; set; }

	}

	[Message(OuterMessage.M2C_TestResponse)]
	[MemoryPackable]
	public partial class M2C_TestResponse: MessageObject, IActorResponse
	{
		public static M2C_TestResponse Create(bool isFromPool = false) { return !isFromPool? new M2C_TestResponse() : NetServices.Instance.FetchMessage(typeof(M2C_TestResponse)) as M2C_TestResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public string response { get; set; }

	}

	[ResponseType(nameof(Actor_TransferResponse))]
	[Message(OuterMessage.Actor_TransferRequest)]
	[MemoryPackable]
	public partial class Actor_TransferRequest: MessageObject, IActorLocationRequest
	{
		public static Actor_TransferRequest Create(bool isFromPool = false) { return !isFromPool? new Actor_TransferRequest() : NetServices.Instance.FetchMessage(typeof(Actor_TransferRequest)) as Actor_TransferRequest; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int MapIndex { get; set; }

	}

	[Message(OuterMessage.Actor_TransferResponse)]
	[MemoryPackable]
	public partial class Actor_TransferResponse: MessageObject, IActorLocationResponse
	{
		public static Actor_TransferResponse Create(bool isFromPool = false) { return !isFromPool? new Actor_TransferResponse() : NetServices.Instance.FetchMessage(typeof(Actor_TransferResponse)) as Actor_TransferResponse; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2C_EnterMap))]
	[Message(OuterMessage.C2G_EnterMap)]
	[MemoryPackable]
	public partial class C2G_EnterMap: MessageObject, IRequest
	{
		public static C2G_EnterMap Create(bool isFromPool = false) { return !isFromPool? new C2G_EnterMap() : NetServices.Instance.FetchMessage(typeof(C2G_EnterMap)) as C2G_EnterMap; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_EnterMap)]
	[MemoryPackable]
	public partial class G2C_EnterMap: MessageObject, IResponse
	{
		public static G2C_EnterMap Create(bool isFromPool = false) { return !isFromPool? new G2C_EnterMap() : NetServices.Instance.FetchMessage(typeof(G2C_EnterMap)) as G2C_EnterMap; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

// 自己unitId
		[MemoryPackOrder(3)]
		public long MyId { get; set; }

	}

	[Message(OuterMessage.MoveInfo)]
	[MemoryPackable]
	public partial class MoveInfo: MessageObject
	{
		public static MoveInfo Create(bool isFromPool = false) { return !isFromPool? new MoveInfo() : NetServices.Instance.FetchMessage(typeof(MoveInfo)) as MoveInfo; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public List<Unity.Mathematics.float3> Points { get; set; }

		[MemoryPackOrder(1)]
		public Unity.Mathematics.quaternion Rotation { get; set; }

		[MemoryPackOrder(2)]
		public int TurnSpeed { get; set; }

	}

	[Message(OuterMessage.UnitInfo)]
	[MemoryPackable]
	public partial class UnitInfo: MessageObject
	{
		public static UnitInfo Create(bool isFromPool = false) { return !isFromPool? new UnitInfo() : NetServices.Instance.FetchMessage(typeof(UnitInfo)) as UnitInfo; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long UnitId { get; set; }

		[MemoryPackOrder(1)]
		public int ConfigId { get; set; }

		[MemoryPackOrder(2)]
		public int Type { get; set; }

		[MemoryPackOrder(3)]
		public Unity.Mathematics.float3 Position { get; set; }

		[MemoryPackOrder(4)]
		public Unity.Mathematics.float3 Forward { get; set; }

		[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
		[MemoryPackOrder(5)]
		public Dictionary<int, long> KV { get; set; }
		[MemoryPackOrder(6)]
		public MoveInfo MoveInfo { get; set; }

	}

	[Message(OuterMessage.M2C_CreateUnits)]
	[MemoryPackable]
	public partial class M2C_CreateUnits: MessageObject, IActorMessage
	{
		public static M2C_CreateUnits Create(bool isFromPool = false) { return !isFromPool? new M2C_CreateUnits() : NetServices.Instance.FetchMessage(typeof(M2C_CreateUnits)) as M2C_CreateUnits; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public List<UnitInfo> Units { get; set; }

	}

	[Message(OuterMessage.M2C_CreateMyUnit)]
	[MemoryPackable]
	public partial class M2C_CreateMyUnit: MessageObject, IActorMessage
	{
		public static M2C_CreateMyUnit Create(bool isFromPool = false) { return !isFromPool? new M2C_CreateMyUnit() : NetServices.Instance.FetchMessage(typeof(M2C_CreateMyUnit)) as M2C_CreateMyUnit; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public UnitInfo Unit { get; set; }

	}

	[Message(OuterMessage.M2C_StartSceneChange)]
	[MemoryPackable]
	public partial class M2C_StartSceneChange: MessageObject, IActorMessage
	{
		public static M2C_StartSceneChange Create(bool isFromPool = false) { return !isFromPool? new M2C_StartSceneChange() : NetServices.Instance.FetchMessage(typeof(M2C_StartSceneChange)) as M2C_StartSceneChange; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long SceneInstanceId { get; set; }

		[MemoryPackOrder(1)]
		public string SceneName { get; set; }

	}

	[Message(OuterMessage.M2C_RemoveUnits)]
	[MemoryPackable]
	public partial class M2C_RemoveUnits: MessageObject, IActorMessage
	{
		public static M2C_RemoveUnits Create(bool isFromPool = false) { return !isFromPool? new M2C_RemoveUnits() : NetServices.Instance.FetchMessage(typeof(M2C_RemoveUnits)) as M2C_RemoveUnits; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public List<long> Units { get; set; }

	}

	[Message(OuterMessage.C2M_PathfindingResult)]
	[MemoryPackable]
	public partial class C2M_PathfindingResult: MessageObject, IActorLocationMessage
	{
		public static C2M_PathfindingResult Create(bool isFromPool = false) { return !isFromPool? new C2M_PathfindingResult() : NetServices.Instance.FetchMessage(typeof(C2M_PathfindingResult)) as C2M_PathfindingResult; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public Unity.Mathematics.float3 Position { get; set; }

	}

	[Message(OuterMessage.C2M_Stop)]
	[MemoryPackable]
	public partial class C2M_Stop: MessageObject, IActorLocationMessage
	{
		public static C2M_Stop Create(bool isFromPool = false) { return !isFromPool? new C2M_Stop() : NetServices.Instance.FetchMessage(typeof(C2M_Stop)) as C2M_Stop; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.M2C_PathfindingResult)]
	[MemoryPackable]
	public partial class M2C_PathfindingResult: MessageObject, IActorMessage
	{
		public static M2C_PathfindingResult Create(bool isFromPool = false) { return !isFromPool? new M2C_PathfindingResult() : NetServices.Instance.FetchMessage(typeof(M2C_PathfindingResult)) as M2C_PathfindingResult; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public long Id { get; set; }

		[MemoryPackOrder(1)]
		public Unity.Mathematics.float3 Position { get; set; }

		[MemoryPackOrder(2)]
		public List<Unity.Mathematics.float3> Points { get; set; }

	}

	[Message(OuterMessage.M2C_Stop)]
	[MemoryPackable]
	public partial class M2C_Stop: MessageObject, IActorMessage
	{
		public static M2C_Stop Create(bool isFromPool = false) { return !isFromPool? new M2C_Stop() : NetServices.Instance.FetchMessage(typeof(M2C_Stop)) as M2C_Stop; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int Error { get; set; }

		[MemoryPackOrder(1)]
		public long Id { get; set; }

		[MemoryPackOrder(2)]
		public Unity.Mathematics.float3 Position { get; set; }

		[MemoryPackOrder(3)]
		public Unity.Mathematics.quaternion Rotation { get; set; }

	}

	[ResponseType(nameof(G2C_Ping))]
	[Message(OuterMessage.C2G_Ping)]
	[MemoryPackable]
	public partial class C2G_Ping: MessageObject, IRequest
	{
		public static C2G_Ping Create(bool isFromPool = false) { return !isFromPool? new C2G_Ping() : NetServices.Instance.FetchMessage(typeof(C2G_Ping)) as C2G_Ping; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Ping)]
	[MemoryPackable]
	public partial class G2C_Ping: MessageObject, IResponse
	{
		public static G2C_Ping Create(bool isFromPool = false) { return !isFromPool? new G2C_Ping() : NetServices.Instance.FetchMessage(typeof(G2C_Ping)) as G2C_Ping; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long Time { get; set; }

	}

	[Message(OuterMessage.G2C_Test)]
	[MemoryPackable]
	public partial class G2C_Test: MessageObject, IMessage
	{
		public static G2C_Test Create(bool isFromPool = false) { return !isFromPool? new G2C_Test() : NetServices.Instance.FetchMessage(typeof(G2C_Test)) as G2C_Test; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

	}

	[ResponseType(nameof(M2C_Reload))]
	[Message(OuterMessage.C2M_Reload)]
	[MemoryPackable]
	public partial class C2M_Reload: MessageObject, IRequest
	{
		public static C2M_Reload Create(bool isFromPool = false) { return !isFromPool? new C2M_Reload() : NetServices.Instance.FetchMessage(typeof(C2M_Reload)) as C2M_Reload; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Account { get; set; }

		[MemoryPackOrder(2)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.M2C_Reload)]
	[MemoryPackable]
	public partial class M2C_Reload: MessageObject, IResponse
	{
		public static M2C_Reload Create(bool isFromPool = false) { return !isFromPool? new M2C_Reload() : NetServices.Instance.FetchMessage(typeof(M2C_Reload)) as M2C_Reload; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(R2C_Login))]
	[Message(OuterMessage.C2R_Login)]
	[MemoryPackable]
	public partial class C2R_Login: MessageObject, IRequest
	{
		public static C2R_Login Create(bool isFromPool = false) { return !isFromPool? new C2R_Login() : NetServices.Instance.FetchMessage(typeof(C2R_Login)) as C2R_Login; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Account { get; set; }

		[MemoryPackOrder(2)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.R2C_Login)]
	[MemoryPackable]
	public partial class R2C_Login: MessageObject, IResponse
	{
		public static R2C_Login Create(bool isFromPool = false) { return !isFromPool? new R2C_Login() : NetServices.Instance.FetchMessage(typeof(R2C_Login)) as R2C_Login; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public string Address { get; set; }

		[MemoryPackOrder(4)]
		public long Key { get; set; }

		[MemoryPackOrder(5)]
		public long GateId { get; set; }

	}

	[ResponseType(nameof(G2C_LoginGate))]
	[Message(OuterMessage.C2G_LoginGate)]
	[MemoryPackable]
	public partial class C2G_LoginGate: MessageObject, IRequest
	{
		public static C2G_LoginGate Create(bool isFromPool = false) { return !isFromPool? new C2G_LoginGate() : NetServices.Instance.FetchMessage(typeof(C2G_LoginGate)) as C2G_LoginGate; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public long Key { get; set; }

		[MemoryPackOrder(2)]
		public long GateId { get; set; }

	}

	[Message(OuterMessage.G2C_LoginGate)]
	[MemoryPackable]
	public partial class G2C_LoginGate: MessageObject, IResponse
	{
		public static G2C_LoginGate Create(bool isFromPool = false) { return !isFromPool? new G2C_LoginGate() : NetServices.Instance.FetchMessage(typeof(G2C_LoginGate)) as G2C_LoginGate; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long PlayerId { get; set; }

	}

	[Message(OuterMessage.G2C_TestHotfixMessage)]
	[MemoryPackable]
	public partial class G2C_TestHotfixMessage: MessageObject, IMessage
	{
		public static G2C_TestHotfixMessage Create(bool isFromPool = false) { return !isFromPool? new G2C_TestHotfixMessage() : NetServices.Instance.FetchMessage(typeof(G2C_TestHotfixMessage)) as G2C_TestHotfixMessage; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public string Info { get; set; }

	}

	[ResponseType(nameof(M2C_TestRobotCase))]
	[Message(OuterMessage.C2M_TestRobotCase)]
	[MemoryPackable]
	public partial class C2M_TestRobotCase: MessageObject, IActorLocationRequest
	{
		public static C2M_TestRobotCase Create(bool isFromPool = false) { return !isFromPool? new C2M_TestRobotCase() : NetServices.Instance.FetchMessage(typeof(C2M_TestRobotCase)) as C2M_TestRobotCase; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int N { get; set; }

	}

	[Message(OuterMessage.M2C_TestRobotCase)]
	[MemoryPackable]
	public partial class M2C_TestRobotCase: MessageObject, IActorLocationResponse
	{
		public static M2C_TestRobotCase Create(bool isFromPool = false) { return !isFromPool? new M2C_TestRobotCase() : NetServices.Instance.FetchMessage(typeof(M2C_TestRobotCase)) as M2C_TestRobotCase; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public int N { get; set; }

	}

	[Message(OuterMessage.C2M_TestRobotCase2)]
	[MemoryPackable]
	public partial class C2M_TestRobotCase2: MessageObject, IActorLocationMessage
	{
		public static C2M_TestRobotCase2 Create(bool isFromPool = false) { return !isFromPool? new C2M_TestRobotCase2() : NetServices.Instance.FetchMessage(typeof(C2M_TestRobotCase2)) as C2M_TestRobotCase2; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int N { get; set; }

	}

	[Message(OuterMessage.M2C_TestRobotCase2)]
	[MemoryPackable]
	public partial class M2C_TestRobotCase2: MessageObject, IActorLocationMessage
	{
		public static M2C_TestRobotCase2 Create(bool isFromPool = false) { return !isFromPool? new M2C_TestRobotCase2() : NetServices.Instance.FetchMessage(typeof(M2C_TestRobotCase2)) as M2C_TestRobotCase2; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int N { get; set; }

	}

	[ResponseType(nameof(M2C_TransferMap))]
	[Message(OuterMessage.C2M_TransferMap)]
	[MemoryPackable]
	public partial class C2M_TransferMap: MessageObject, IActorLocationRequest
	{
		public static C2M_TransferMap Create(bool isFromPool = false) { return !isFromPool? new C2M_TransferMap() : NetServices.Instance.FetchMessage(typeof(C2M_TransferMap)) as C2M_TransferMap; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.M2C_TransferMap)]
	[MemoryPackable]
	public partial class M2C_TransferMap: MessageObject, IActorLocationResponse
	{
		public static M2C_TransferMap Create(bool isFromPool = false) { return !isFromPool? new M2C_TransferMap() : NetServices.Instance.FetchMessage(typeof(M2C_TransferMap)) as M2C_TransferMap; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2C_Benchmark))]
	[Message(OuterMessage.C2G_Benchmark)]
	[MemoryPackable]
	public partial class C2G_Benchmark: MessageObject, IRequest
	{
		public static C2G_Benchmark Create(bool isFromPool = false) { return !isFromPool? new C2G_Benchmark() : NetServices.Instance.FetchMessage(typeof(C2G_Benchmark)) as C2G_Benchmark; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Benchmark)]
	[MemoryPackable]
	public partial class G2C_Benchmark: MessageObject, IResponse
	{
		public static G2C_Benchmark Create(bool isFromPool = false) { return !isFromPool? new G2C_Benchmark() : NetServices.Instance.FetchMessage(typeof(G2C_Benchmark)) as G2C_Benchmark; }

		public override void Dispose() { NetServices.Instance.RecycleMessage(this); }

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

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
