using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(LockStepOuter.C2G_Match)]
    [ResponseType(nameof(G2C_Match))]
    public partial class C2G_Match : MessageObject, ISessionRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static C2G_Match Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_Match), isFromPool) as C2G_Match;
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
    [Message(LockStepOuter.G2C_Match)]
    public partial class G2C_Match : MessageObject, ISessionResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static G2C_Match Create(int error = default, string message = default, bool isFromPool = false)
        {
            G2C_Match msg = ObjectPool.Instance.Fetch(typeof(G2C_Match), isFromPool) as G2C_Match;
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

    /// <summary>
    /// 匹配成功，通知客户端切换场景
    /// </summary>
    [MemoryPackable]
    [Message(LockStepOuter.Match2G_NotifyMatchSuccess)]
    public partial class Match2G_NotifyMatchSuccess : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 房间的ActorId
        /// </summary>
        [MemoryPackOrder(1)]
        public ActorId ActorId { get; set; }

        /// <summary>
        /// Create Match2G_NotifyMatchSuccess
        /// </summary>
        /// <param name="rpcId">RpcId</param>
        /// <param name="actorId">房间的ActorId</param>
        /// <param name="isFromPool"></param>
        public static Match2G_NotifyMatchSuccess Create(int rpcId = default, ActorId actorId = default, bool isFromPool = false)
        {
            Match2G_NotifyMatchSuccess msg = ObjectPool.Instance.Fetch(typeof(Match2G_NotifyMatchSuccess), isFromPool) as Match2G_NotifyMatchSuccess;
            msg.Set(rpcId, actorId);
            return msg;
        }

        /// <summary>
        /// Set Match2G_NotifyMatchSuccess
        /// </summary>
        /// <param name="rpcId">RpcId</param>
        /// <param name="actorId">房间的ActorId</param>
        public void Set(int rpcId = default, ActorId actorId = default)
        {
            this.RpcId = rpcId;
            this.ActorId = actorId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 客户端通知房间切换场景完成
    /// </summary>
    [MemoryPackable]
    [Message(LockStepOuter.C2Room_ChangeSceneFinish)]
    public partial class C2Room_ChangeSceneFinish : MessageObject, IRoomMessage
    {
        [MemoryPackOrder(0)]
        public long PlayerId { get; set; }

        public static C2Room_ChangeSceneFinish Create(long playerId = default, bool isFromPool = false)
        {
            C2Room_ChangeSceneFinish msg = ObjectPool.Instance.Fetch(typeof(C2Room_ChangeSceneFinish), isFromPool) as C2Room_ChangeSceneFinish;
            msg.Set(playerId);
            return msg;
        }

        public void Set(long playerId = default)
        {
            this.PlayerId = playerId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.PlayerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepOuter.LockStepUnitInfo)]
    public partial class LockStepUnitInfo : MessageObject
    {
        [MemoryPackOrder(0)]
        public long PlayerId { get; set; }

        [MemoryPackOrder(1)]
        public TrueSync.TSVector Position { get; set; }

        [MemoryPackOrder(2)]
        public TrueSync.TSQuaternion Rotation { get; set; }

        public static LockStepUnitInfo Create(long playerId = default, TrueSync.TSVector position = default, TrueSync.TSQuaternion rotation = default, bool isFromPool = false)
        {
            LockStepUnitInfo msg = ObjectPool.Instance.Fetch(typeof(LockStepUnitInfo), isFromPool) as LockStepUnitInfo;
            msg.Set(playerId, position, rotation);
            return msg;
        }

        public void Set(long playerId = default, TrueSync.TSVector position = default, TrueSync.TSQuaternion rotation = default)
        {
            this.PlayerId = playerId;
            this.Position = position;
            this.Rotation = rotation;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.PlayerId = default;
            this.Position = default;
            this.Rotation = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 房间通知客户端进入战斗
    /// </summary>
    [MemoryPackable]
    [Message(LockStepOuter.Room2C_Start)]
    public partial class Room2C_Start : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public long StartTime { get; set; }

        [MemoryPackOrder(1)]
        public List<LockStepUnitInfo> UnitInfo { get; set; } = new();

        public static Room2C_Start Create(long startTime = default, bool isFromPool = false)
        {
            Room2C_Start msg = ObjectPool.Instance.Fetch(typeof(Room2C_Start), isFromPool) as Room2C_Start;
            msg.Set(startTime);
            return msg;
        }

        public void Set(long startTime = default)
        {
            this.StartTime = startTime;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.StartTime = default;
            this.UnitInfo.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepOuter.FrameMessage)]
    public partial class FrameMessage : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public int Frame { get; set; }

        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }

        [MemoryPackOrder(2)]
        public LSInput Input { get; set; }

        public static FrameMessage Create(int frame = default, long playerId = default, LSInput input = default, bool isFromPool = false)
        {
            FrameMessage msg = ObjectPool.Instance.Fetch(typeof(FrameMessage), isFromPool) as FrameMessage;
            msg.Set(frame, playerId, input);
            return msg;
        }

        public void Set(int frame = default, long playerId = default, LSInput input = default)
        {
            this.Frame = frame;
            this.PlayerId = playerId;
            this.Input = input;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Frame = default;
            this.PlayerId = default;
            this.Input = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepOuter.OneFrameInputs)]
    public partial class OneFrameInputs : MessageObject, IMessage
    {
        [MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
        [MemoryPackOrder(1)]
        public Dictionary<long, LSInput> Inputs { get; set; } = new();

        public static OneFrameInputs Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(OneFrameInputs), isFromPool) as OneFrameInputs;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Inputs.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepOuter.Room2C_AdjustUpdateTime)]
    public partial class Room2C_AdjustUpdateTime : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public int DiffTime { get; set; }

        public static Room2C_AdjustUpdateTime Create(int diffTime = default, bool isFromPool = false)
        {
            Room2C_AdjustUpdateTime msg = ObjectPool.Instance.Fetch(typeof(Room2C_AdjustUpdateTime), isFromPool) as Room2C_AdjustUpdateTime;
            msg.Set(diffTime);
            return msg;
        }

        public void Set(int diffTime = default)
        {
            this.DiffTime = diffTime;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.DiffTime = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepOuter.C2Room_CheckHash)]
    public partial class C2Room_CheckHash : MessageObject, IRoomMessage
    {
        [MemoryPackOrder(0)]
        public long PlayerId { get; set; }

        [MemoryPackOrder(1)]
        public int Frame { get; set; }

        [MemoryPackOrder(2)]
        public long Hash { get; set; }

        public static C2Room_CheckHash Create(long playerId = default, int frame = default, long hash = default, bool isFromPool = false)
        {
            C2Room_CheckHash msg = ObjectPool.Instance.Fetch(typeof(C2Room_CheckHash), isFromPool) as C2Room_CheckHash;
            msg.Set(playerId, frame, hash);
            return msg;
        }

        public void Set(long playerId = default, int frame = default, long hash = default)
        {
            this.PlayerId = playerId;
            this.Frame = frame;
            this.Hash = hash;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.PlayerId = default;
            this.Frame = default;
            this.Hash = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepOuter.Room2C_CheckHashFail)]
    public partial class Room2C_CheckHashFail : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public int Frame { get; set; }

        [MemoryPackOrder(1)]
        public byte[] LSWorldBytes { get; set; }

        public static Room2C_CheckHashFail Create(int frame = default, byte[] lSWorldBytes = default, bool isFromPool = false)
        {
            Room2C_CheckHashFail msg = ObjectPool.Instance.Fetch(typeof(Room2C_CheckHashFail), isFromPool) as Room2C_CheckHashFail;
            msg.Set(frame, lSWorldBytes);
            return msg;
        }

        public void Set(int frame = default, byte[] lSWorldBytes = default)
        {
            this.Frame = frame;
            this.LSWorldBytes = lSWorldBytes;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Frame = default;
            this.LSWorldBytes = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepOuter.G2C_Reconnect)]
    public partial class G2C_Reconnect : MessageObject, IMessage
    {
        [MemoryPackOrder(0)]
        public long StartTime { get; set; }

        [MemoryPackOrder(1)]
        public List<LockStepUnitInfo> UnitInfos { get; set; } = new();

        [MemoryPackOrder(2)]
        public int Frame { get; set; }

        public static G2C_Reconnect Create(long startTime = default, int frame = default, bool isFromPool = false)
        {
            G2C_Reconnect msg = ObjectPool.Instance.Fetch(typeof(G2C_Reconnect), isFromPool) as G2C_Reconnect;
            msg.Set(startTime, frame);
            return msg;
        }

        public void Set(long startTime = default, int frame = default)
        {
            this.StartTime = startTime;
            this.Frame = frame;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.StartTime = default;
            this.UnitInfos.Clear();
            this.Frame = default;

            ObjectPool.Instance.Recycle(this);
        }
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