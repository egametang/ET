using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 请求匹配
    /// </summary>
    [MemoryPackable]
    [Message(LockStepInner.G2Match_Match)]
    [ResponseType(nameof(Match2G_Match))]
    public partial class G2Match_Match : MessageObject, IRequest
    {
        public static G2Match_Match Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Match_Match), isFromPool) as G2Match_Match;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Id { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Id = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepInner.Match2G_Match)]
    public partial class Match2G_Match : MessageObject, IResponse
    {
        public static Match2G_Match Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Match2G_Match), isFromPool) as Match2G_Match;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

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
    [Message(LockStepInner.Match2Map_GetRoom)]
    [ResponseType(nameof(Map2Match_GetRoom))]
    public partial class Match2Map_GetRoom : MessageObject, IRequest
    {
        public static Match2Map_GetRoom Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Match2Map_GetRoom), isFromPool) as Match2Map_GetRoom;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public List<long> PlayerIds { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerIds.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepInner.Map2Match_GetRoom)]
    public partial class Map2Match_GetRoom : MessageObject, IResponse
    {
        public static Map2Match_GetRoom Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Map2Match_GetRoom), isFromPool) as Map2Match_GetRoom;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        /// <summary>
        /// 房间的ActorId
        /// </summary>
        [MemoryPackOrder(3)]
        public ActorId ActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepInner.G2Room_Reconnect)]
    [ResponseType(nameof(Room2G_Reconnect))]
    public partial class G2Room_Reconnect : MessageObject, IRequest
    {
        public static G2Room_Reconnect Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Room_Reconnect), isFromPool) as G2Room_Reconnect;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepInner.Room2G_Reconnect)]
    public partial class Room2G_Reconnect : MessageObject, IResponse
    {
        public static Room2G_Reconnect Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Room2G_Reconnect), isFromPool) as Room2G_Reconnect;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long StartTime { get; set; }

        [MemoryPackOrder(4)]
        public List<LockStepUnitInfo> UnitInfos { get; set; } = new();

        [MemoryPackOrder(5)]
        public int Frame { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.StartTime = default;
            this.UnitInfos.Clear();
            this.Frame = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepInner.RoomManager2Room_Init)]
    [ResponseType(nameof(Room2RoomManager_Init))]
    public partial class RoomManager2Room_Init : MessageObject, IRequest
    {
        public static RoomManager2Room_Init Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(RoomManager2Room_Init), isFromPool) as RoomManager2Room_Init;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public List<long> PlayerIds { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.PlayerIds.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(LockStepInner.Room2RoomManager_Init)]
    public partial class Room2RoomManager_Init : MessageObject, IResponse
    {
        public static Room2RoomManager_Init Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Room2RoomManager_Init), isFromPool) as Room2RoomManager_Init;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

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

    public static class LockStepInner
    {
        public const ushort G2Match_Match = 21002;
        public const ushort Match2G_Match = 21003;
        public const ushort Match2Map_GetRoom = 21004;
        public const ushort Map2Match_GetRoom = 21005;
        public const ushort G2Room_Reconnect = 21006;
        public const ushort Room2G_Reconnect = 21007;
        public const ushort RoomManager2Room_Init = 21008;
        public const ushort Room2RoomManager_Init = 21009;
    }
}