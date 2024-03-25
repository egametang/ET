using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(OuterMessage.HttpGetRouterResponse)]
    public partial class HttpGetRouterResponse : MessageObject
    {
        [MemoryPackOrder(0)]
        public List<string> Realms { get; set; } = new();

        [MemoryPackOrder(1)]
        public List<string> Routers { get; set; } = new();

        public static HttpGetRouterResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(HttpGetRouterResponse), isFromPool) as HttpGetRouterResponse;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Realms.Clear();
            this.Routers.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.RouterSync)]
    public partial class RouterSync : MessageObject
    {
        [MemoryPackOrder(0)]
        public uint ConnectId { get; set; }

        [MemoryPackOrder(1)]
        public string Address { get; set; }

        public static RouterSync Create(uint connectId = default, string address = default, bool isFromPool = false)
        {
            RouterSync msg = ObjectPool.Instance.Fetch(typeof(RouterSync), isFromPool) as RouterSync;
            msg.Set(connectId, address);
            return msg;
        }

        public void Set(uint connectId = default, string address = default)
        {
            this.ConnectId = connectId;
            this.Address = address;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.ConnectId = default;
            this.Address = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TestRequest)]
    [ResponseType(nameof(M2C_TestResponse))]
    public partial class C2M_TestRequest : MessageObject, ILocationRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string request { get; set; }

        public static C2M_TestRequest Create(string request = default, bool isFromPool = false)
        {
            C2M_TestRequest msg = ObjectPool.Instance.Fetch(typeof(C2M_TestRequest), isFromPool) as C2M_TestRequest;
            msg.Set(request);
            return msg;
        }

        public void Set(string request = default)
        {
            this.request = request;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.request = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TestResponse)]
    public partial class M2C_TestResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public string response { get; set; }

        public static M2C_TestResponse Create(int error = default, string message = default, string response = default, bool isFromPool = false)
        {
            M2C_TestResponse msg = ObjectPool.Instance.Fetch(typeof(M2C_TestResponse), isFromPool) as M2C_TestResponse;
            msg.Set(error, message, response);
            return msg;
        }

        public void Set(int error = default, string message = default, string response = default)
        {
            this.Error = error;
            this.Message = message;
            this.response = response;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.response = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_EnterMap)]
    [ResponseType(nameof(G2C_EnterMap))]
    public partial class C2G_EnterMap : MessageObject, ISessionRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static C2G_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_EnterMap), isFromPool) as C2G_EnterMap;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_EnterMap)]
    public partial class G2C_EnterMap : MessageObject, ISessionResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        /// <summary>
        /// 自己的UnitId
        /// </summary>
        [MemoryPackOrder(3)]
        public long MyId { get; set; }

        /// <summary>
        /// Create G2C_EnterMap
        /// </summary>
        /// <param name="error">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="myId">自己的UnitId</param>
        /// <param name="isFromPool"></param>
        public static G2C_EnterMap Create(int error = default, string message = default, long myId = default, bool isFromPool = false)
        {
            G2C_EnterMap msg = ObjectPool.Instance.Fetch(typeof(G2C_EnterMap), isFromPool) as G2C_EnterMap;
            msg.Set(error, message, myId);
            return msg;
        }

        /// <summary>
        /// Set G2C_EnterMap
        /// </summary>
        /// <param name="error">错误码</param>
        /// <param name="message">错误消息</param>
        /// <param name="myId">自己的UnitId</param>
        public void Set(int error = default, string message = default, long myId = default)
        {
            this.Error = error;
            this.Message = message;
            this.MyId = myId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MyId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.MoveInfo)]
    public partial class MoveInfo : MessageObject
    {
        [MemoryPackOrder(0)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        [MemoryPackOrder(1)]
        public Unity.Mathematics.quaternion Rotation { get; set; }

        [MemoryPackOrder(2)]
        public int TurnSpeed { get; set; }

        public static MoveInfo Create(Unity.Mathematics.quaternion rotation = default, int turnSpeed = default, bool isFromPool = false)
        {
            MoveInfo msg = ObjectPool.Instance.Fetch(typeof(MoveInfo), isFromPool) as MoveInfo;
            msg.Set(rotation, turnSpeed);
            return msg;
        }

        public void Set(Unity.Mathematics.quaternion rotation = default, int turnSpeed = default)
        {
            this.Rotation = rotation;
            this.TurnSpeed = turnSpeed;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Points.Clear();
            this.Rotation = default;
            this.TurnSpeed = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.UnitInfo)]
    public partial class UnitInfo : MessageObject
    {
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
        public Dictionary<int, long> KV { get; set; } = new();

        [MemoryPackOrder(6)]
        public MoveInfo MoveInfo { get; set; }

        public static UnitInfo Create(long unitId = default, int configId = default, int type = default, Unity.Mathematics.float3 position = default, Unity.Mathematics.float3 forward = default, MoveInfo moveInfo = default, bool isFromPool = false)
        {
            UnitInfo msg = ObjectPool.Instance.Fetch(typeof(UnitInfo), isFromPool) as UnitInfo;
            msg.Set(unitId, configId, type, position, forward, moveInfo);
            return msg;
        }

        public void Set(long unitId = default, int configId = default, int type = default, Unity.Mathematics.float3 position = default, Unity.Mathematics.float3 forward = default, MoveInfo moveInfo = default)
        {
            this.UnitId = unitId;
            this.ConfigId = configId;
            this.Type = type;
            this.Position = position;
            this.Forward = forward;
            this.MoveInfo = moveInfo;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.UnitId = default;
            this.ConfigId = default;
            this.Type = default;
            this.Position = default;
            this.Forward = default;
            this.KV.Clear();
            this.MoveInfo = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_CreateUnits)]
    public partial class M2C_CreateUnits : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public List<UnitInfo> Units { get; set; } = new();

        public static M2C_CreateUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_CreateUnits), isFromPool) as M2C_CreateUnits;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Units.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_CreateMyUnit)]
    public partial class M2C_CreateMyUnit : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public UnitInfo Unit { get; set; }

        public static M2C_CreateMyUnit Create(UnitInfo unit = default, bool isFromPool = false)
        {
            M2C_CreateMyUnit msg = ObjectPool.Instance.Fetch(typeof(M2C_CreateMyUnit), isFromPool) as M2C_CreateMyUnit;
            msg.Set(unit);
            return msg;
        }

        public void Set(UnitInfo unit = default)
        {
            this.Unit = unit;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Unit = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_StartSceneChange)]
    public partial class M2C_StartSceneChange : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public long SceneInstanceId { get; set; }

        [MemoryPackOrder(1)]
        public string SceneName { get; set; }

        public static M2C_StartSceneChange Create(long sceneInstanceId = default, string sceneName = default, bool isFromPool = false)
        {
            M2C_StartSceneChange msg = ObjectPool.Instance.Fetch(typeof(M2C_StartSceneChange), isFromPool) as M2C_StartSceneChange;
            msg.Set(sceneInstanceId, sceneName);
            return msg;
        }

        public void Set(long sceneInstanceId = default, string sceneName = default)
        {
            this.SceneInstanceId = sceneInstanceId;
            this.SceneName = sceneName;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.SceneInstanceId = default;
            this.SceneName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_RemoveUnits)]
    public partial class M2C_RemoveUnits : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public List<long> Units { get; set; } = new();

        public static M2C_RemoveUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_RemoveUnits), isFromPool) as M2C_RemoveUnits;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Units.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_PathfindingResult)]
    public partial class C2M_PathfindingResult : MessageObject, ILocationMessage
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public Unity.Mathematics.float3 Position { get; set; }

        public static C2M_PathfindingResult Create(Unity.Mathematics.float3 position = default, bool isFromPool = false)
        {
            C2M_PathfindingResult msg = ObjectPool.Instance.Fetch(typeof(C2M_PathfindingResult), isFromPool) as C2M_PathfindingResult;
            msg.Set(position);
            return msg;
        }

        public void Set(Unity.Mathematics.float3 position = default)
        {
            this.Position = position;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Position = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_Stop)]
    public partial class C2M_Stop : MessageObject, ILocationMessage
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static C2M_Stop Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_Stop), isFromPool) as C2M_Stop;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_PathfindingResult)]
    public partial class M2C_PathfindingResult : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public long Id { get; set; }

        [MemoryPackOrder(1)]
        public Unity.Mathematics.float3 Position { get; set; }

        [MemoryPackOrder(2)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        public static M2C_PathfindingResult Create(long id = default, Unity.Mathematics.float3 position = default, bool isFromPool = false)
        {
            M2C_PathfindingResult msg = ObjectPool.Instance.Fetch(typeof(M2C_PathfindingResult), isFromPool) as M2C_PathfindingResult;
            msg.Set(id, position);
            return msg;
        }

        public void Set(long id = default, Unity.Mathematics.float3 position = default)
        {
            this.Id = id;
            this.Position = position;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.Position = default;
            this.Points.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_Stop)]
    public partial class M2C_Stop : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public int Error { get; set; }

        [MemoryPackOrder(1)]
        public long Id { get; set; }

        [MemoryPackOrder(2)]
        public Unity.Mathematics.float3 Position { get; set; }

        [MemoryPackOrder(3)]
        public Unity.Mathematics.quaternion Rotation { get; set; }

        public static M2C_Stop Create(int error = default, long id = default, Unity.Mathematics.float3 position = default, Unity.Mathematics.quaternion rotation = default, bool isFromPool = false)
        {
            M2C_Stop msg = ObjectPool.Instance.Fetch(typeof(M2C_Stop), isFromPool) as M2C_Stop;
            msg.Set(error, id, position, rotation);
            return msg;
        }

        public void Set(int error = default, long id = default, Unity.Mathematics.float3 position = default, Unity.Mathematics.quaternion rotation = default)
        {
            this.Error = error;
            this.Id = id;
            this.Position = position;
            this.Rotation = rotation;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Error = default;
            this.Id = default;
            this.Position = default;
            this.Rotation = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_Ping)]
    [ResponseType(nameof(G2C_Ping))]
    public partial class C2G_Ping : MessageObject, ISessionRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static C2G_Ping Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_Ping), isFromPool) as C2G_Ping;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_Ping)]
    public partial class G2C_Ping : MessageObject, ISessionResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long Time { get; set; }

        public static G2C_Ping Create(int error = default, string message = default, long time = default, bool isFromPool = false)
        {
            G2C_Ping msg = ObjectPool.Instance.Fetch(typeof(G2C_Ping), isFromPool) as G2C_Ping;
            msg.Set(error, message, time);
            return msg;
        }

        public void Set(int error = default, string message = default, long time = default)
        {
            this.Error = error;
            this.Message = message;
            this.Time = time;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Time = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_Test)]
    public partial class G2C_Test : MessageObject, ISessionMessage
    {
        public static G2C_Test Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_Test), isFromPool) as G2C_Test;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_Reload)]
    [ResponseType(nameof(M2C_Reload))]
    public partial class C2M_Reload : MessageObject, ISessionRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Account { get; set; }

        [MemoryPackOrder(2)]
        public string Password { get; set; }

        public static C2M_Reload Create(string account = default, string password = default, bool isFromPool = false)
        {
            C2M_Reload msg = ObjectPool.Instance.Fetch(typeof(C2M_Reload), isFromPool) as C2M_Reload;
            msg.Set(account, password);
            return msg;
        }

        public void Set(string account = default, string password = default)
        {
            this.Account = account;
            this.Password = password;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Account = default;
            this.Password = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_Reload)]
    public partial class M2C_Reload : MessageObject, ISessionResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static M2C_Reload Create(int error = default, string message = default, bool isFromPool = false)
        {
            M2C_Reload msg = ObjectPool.Instance.Fetch(typeof(M2C_Reload), isFromPool) as M2C_Reload;
            msg.Set(error, message);
            return msg;
        }

        public void Set(int error = default, string message = default)
        {
            this.Error = error;
            this.Message = message;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2R_Login)]
    [ResponseType(nameof(R2C_Login))]
    public partial class C2R_Login : MessageObject, ISessionRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        [MemoryPackOrder(1)]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MemoryPackOrder(2)]
        public string Password { get; set; }

        /// <summary>
        /// Create C2R_Login
        /// </summary>
        /// <param name="account">帐号</param>
        /// <param name="password">密码</param>
        /// <param name="isFromPool"></param>
        public static C2R_Login Create(string account = default, string password = default, bool isFromPool = false)
        {
            C2R_Login msg = ObjectPool.Instance.Fetch(typeof(C2R_Login), isFromPool) as C2R_Login;
            msg.Set(account, password);
            return msg;
        }

        /// <summary>
        /// Set C2R_Login
        /// </summary>
        /// <param name="account">帐号</param>
        /// <param name="password">密码</param>
        public void Set(string account = default, string password = default)
        {
            this.Account = account;
            this.Password = password;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Account = default;
            this.Password = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_Login)]
    public partial class R2C_Login : MessageObject, ISessionResponse
    {
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

        public static R2C_Login Create(int error = default, string message = default, string address = default, long key = default, long gateId = default, bool isFromPool = false)
        {
            R2C_Login msg = ObjectPool.Instance.Fetch(typeof(R2C_Login), isFromPool) as R2C_Login;
            msg.Set(error, message, address, key, gateId);
            return msg;
        }

        public void Set(int error = default, string message = default, string address = default, long key = default, long gateId = default)
        {
            this.Error = error;
            this.Message = message;
            this.Address = address;
            this.Key = key;
            this.GateId = gateId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Address = default;
            this.Key = default;
            this.GateId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_LoginGate)]
    [ResponseType(nameof(G2C_LoginGate))]
    public partial class C2G_LoginGate : MessageObject, ISessionRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        [MemoryPackOrder(1)]
        public long Key { get; set; }

        [MemoryPackOrder(2)]
        public long GateId { get; set; }

        /// <summary>
        /// Create C2G_LoginGate
        /// </summary>
        /// <param name="key">帐号</param>
        /// <param name="gateId">GateId</param>
        /// <param name="isFromPool"></param>
        public static C2G_LoginGate Create(long key = default, long gateId = default, bool isFromPool = false)
        {
            C2G_LoginGate msg = ObjectPool.Instance.Fetch(typeof(C2G_LoginGate), isFromPool) as C2G_LoginGate;
            msg.Set(key, gateId);
            return msg;
        }

        /// <summary>
        /// Set C2G_LoginGate
        /// </summary>
        /// <param name="key">帐号</param>
        /// <param name="gateId">GateId</param>
        public void Set(long key = default, long gateId = default)
        {
            this.Key = key;
            this.GateId = gateId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Key = default;
            this.GateId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_LoginGate)]
    public partial class G2C_LoginGate : MessageObject, ISessionResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long PlayerId { get; set; }

        public static G2C_LoginGate Create(int error = default, string message = default, long playerId = default, bool isFromPool = false)
        {
            G2C_LoginGate msg = ObjectPool.Instance.Fetch(typeof(G2C_LoginGate), isFromPool) as G2C_LoginGate;
            msg.Set(error, message, playerId);
            return msg;
        }

        public void Set(int error = default, string message = default, long playerId = default)
        {
            this.Error = error;
            this.Message = message;
            this.PlayerId = playerId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.PlayerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_TestHotfixMessage)]
    public partial class G2C_TestHotfixMessage : MessageObject, ISessionMessage
    {
        [MemoryPackOrder(0)]
        public string Info { get; set; }

        public static G2C_TestHotfixMessage Create(string info = default, bool isFromPool = false)
        {
            G2C_TestHotfixMessage msg = ObjectPool.Instance.Fetch(typeof(G2C_TestHotfixMessage), isFromPool) as G2C_TestHotfixMessage;
            msg.Set(info);
            return msg;
        }

        public void Set(string info = default)
        {
            this.Info = info;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Info = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TestRobotCase)]
    [ResponseType(nameof(M2C_TestRobotCase))]
    public partial class C2M_TestRobotCase : MessageObject, ILocationRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int N { get; set; }

        public static C2M_TestRobotCase Create(int n = default, bool isFromPool = false)
        {
            C2M_TestRobotCase msg = ObjectPool.Instance.Fetch(typeof(C2M_TestRobotCase), isFromPool) as C2M_TestRobotCase;
            msg.Set(n);
            return msg;
        }

        public void Set(int n = default)
        {
            this.N = n;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TestRobotCase)]
    public partial class M2C_TestRobotCase : MessageObject, ILocationResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public int N { get; set; }

        public static M2C_TestRobotCase Create(int error = default, string message = default, int n = default, bool isFromPool = false)
        {
            M2C_TestRobotCase msg = ObjectPool.Instance.Fetch(typeof(M2C_TestRobotCase), isFromPool) as M2C_TestRobotCase;
            msg.Set(error, message, n);
            return msg;
        }

        public void Set(int error = default, string message = default, int n = default)
        {
            this.Error = error;
            this.Message = message;
            this.N = n;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TestRobotCase2)]
    public partial class C2M_TestRobotCase2 : MessageObject, ILocationMessage
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int N { get; set; }

        public static C2M_TestRobotCase2 Create(int n = default, bool isFromPool = false)
        {
            C2M_TestRobotCase2 msg = ObjectPool.Instance.Fetch(typeof(C2M_TestRobotCase2), isFromPool) as C2M_TestRobotCase2;
            msg.Set(n);
            return msg;
        }

        public void Set(int n = default)
        {
            this.N = n;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TestRobotCase2)]
    public partial class M2C_TestRobotCase2 : MessageObject, ILocationMessage
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int N { get; set; }

        public static M2C_TestRobotCase2 Create(int n = default, bool isFromPool = false)
        {
            M2C_TestRobotCase2 msg = ObjectPool.Instance.Fetch(typeof(M2C_TestRobotCase2), isFromPool) as M2C_TestRobotCase2;
            msg.Set(n);
            return msg;
        }

        public void Set(int n = default)
        {
            this.N = n;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TransferMap)]
    [ResponseType(nameof(M2C_TransferMap))]
    public partial class C2M_TransferMap : MessageObject, ILocationRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static C2M_TransferMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_TransferMap), isFromPool) as C2M_TransferMap;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TransferMap)]
    public partial class M2C_TransferMap : MessageObject, ILocationResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static M2C_TransferMap Create(int error = default, string message = default, bool isFromPool = false)
        {
            M2C_TransferMap msg = ObjectPool.Instance.Fetch(typeof(M2C_TransferMap), isFromPool) as M2C_TransferMap;
            msg.Set(error, message);
            return msg;
        }

        public void Set(int error = default, string message = default)
        {
            this.Error = error;
            this.Message = message;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_Benchmark)]
    [ResponseType(nameof(G2C_Benchmark))]
    public partial class C2G_Benchmark : MessageObject, ISessionRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static C2G_Benchmark Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_Benchmark), isFromPool) as C2G_Benchmark;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_Benchmark)]
    public partial class G2C_Benchmark : MessageObject, ISessionResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static G2C_Benchmark Create(int error = default, string message = default, bool isFromPool = false)
        {
            G2C_Benchmark msg = ObjectPool.Instance.Fetch(typeof(G2C_Benchmark), isFromPool) as G2C_Benchmark;
            msg.Set(error, message);
            return msg;
        }

        public void Set(int error = default, string message = default)
        {
            this.Error = error;
            this.Message = message;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    public static class OuterMessage
    {
        public const ushort HttpGetRouterResponse = 10002;
        public const ushort RouterSync = 10003;
        public const ushort C2M_TestRequest = 10004;
        public const ushort M2C_TestResponse = 10005;
        public const ushort C2G_EnterMap = 10006;
        public const ushort G2C_EnterMap = 10007;
        public const ushort MoveInfo = 10008;
        public const ushort UnitInfo = 10009;
        public const ushort M2C_CreateUnits = 10010;
        public const ushort M2C_CreateMyUnit = 10011;
        public const ushort M2C_StartSceneChange = 10012;
        public const ushort M2C_RemoveUnits = 10013;
        public const ushort C2M_PathfindingResult = 10014;
        public const ushort C2M_Stop = 10015;
        public const ushort M2C_PathfindingResult = 10016;
        public const ushort M2C_Stop = 10017;
        public const ushort C2G_Ping = 10018;
        public const ushort G2C_Ping = 10019;
        public const ushort G2C_Test = 10020;
        public const ushort C2M_Reload = 10021;
        public const ushort M2C_Reload = 10022;
        public const ushort C2R_Login = 10023;
        public const ushort R2C_Login = 10024;
        public const ushort C2G_LoginGate = 10025;
        public const ushort G2C_LoginGate = 10026;
        public const ushort G2C_TestHotfixMessage = 10027;
        public const ushort C2M_TestRobotCase = 10028;
        public const ushort M2C_TestRobotCase = 10029;
        public const ushort C2M_TestRobotCase2 = 10030;
        public const ushort M2C_TestRobotCase2 = 10031;
        public const ushort C2M_TransferMap = 10032;
        public const ushort M2C_TransferMap = 10033;
        public const ushort C2G_Benchmark = 10034;
        public const ushort G2C_Benchmark = 10035;
    }
}