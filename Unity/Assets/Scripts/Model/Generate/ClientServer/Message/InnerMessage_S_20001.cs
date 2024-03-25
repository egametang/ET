using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(InnerMessage.ObjectQueryRequest)]
    [ResponseType(nameof(ObjectQueryResponse))]
    public partial class ObjectQueryRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Key { get; set; }

        [MemoryPackOrder(2)]
        public long InstanceId { get; set; }

        public static ObjectQueryRequest Create(long key = default, long instanceId = default, bool isFromPool = false)
        {
            ObjectQueryRequest msg = ObjectPool.Instance.Fetch(typeof(ObjectQueryRequest), isFromPool) as ObjectQueryRequest;
            msg.Set(key, instanceId);
            return msg;
        }

        public void Set(long key = default, long instanceId = default)
        {
            this.Key = key;
            this.InstanceId = instanceId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Key = default;
            this.InstanceId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2A_Reload)]
    [ResponseType(nameof(A2M_Reload))]
    public partial class M2A_Reload : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static M2A_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2A_Reload), isFromPool) as M2A_Reload;
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
    [Message(InnerMessage.A2M_Reload)]
    public partial class A2M_Reload : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static A2M_Reload Create(int error = default, string message = default, bool isFromPool = false)
        {
            A2M_Reload msg = ObjectPool.Instance.Fetch(typeof(A2M_Reload), isFromPool) as A2M_Reload;
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
    [Message(InnerMessage.G2G_LockRequest)]
    [ResponseType(nameof(G2G_LockResponse))]
    public partial class G2G_LockRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Id { get; set; }

        [MemoryPackOrder(2)]
        public string Address { get; set; }

        public static G2G_LockRequest Create(long id = default, string address = default, bool isFromPool = false)
        {
            G2G_LockRequest msg = ObjectPool.Instance.Fetch(typeof(G2G_LockRequest), isFromPool) as G2G_LockRequest;
            msg.Set(id, address);
            return msg;
        }

        public void Set(long id = default, string address = default)
        {
            this.Id = id;
            this.Address = address;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Id = default;
            this.Address = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2G_LockResponse)]
    public partial class G2G_LockResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static G2G_LockResponse Create(int error = default, string message = default, bool isFromPool = false)
        {
            G2G_LockResponse msg = ObjectPool.Instance.Fetch(typeof(G2G_LockResponse), isFromPool) as G2G_LockResponse;
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
    [Message(InnerMessage.G2G_LockReleaseRequest)]
    [ResponseType(nameof(G2G_LockReleaseResponse))]
    public partial class G2G_LockReleaseRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Id { get; set; }

        [MemoryPackOrder(2)]
        public string Address { get; set; }

        public static G2G_LockReleaseRequest Create(long id = default, string address = default, bool isFromPool = false)
        {
            G2G_LockReleaseRequest msg = ObjectPool.Instance.Fetch(typeof(G2G_LockReleaseRequest), isFromPool) as G2G_LockReleaseRequest;
            msg.Set(id, address);
            return msg;
        }

        public void Set(long id = default, string address = default)
        {
            this.Id = id;
            this.Address = address;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Id = default;
            this.Address = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2G_LockReleaseResponse)]
    public partial class G2G_LockReleaseResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static G2G_LockReleaseResponse Create(int error = default, string message = default, bool isFromPool = false)
        {
            G2G_LockReleaseResponse msg = ObjectPool.Instance.Fetch(typeof(G2G_LockReleaseResponse), isFromPool) as G2G_LockReleaseResponse;
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
    [Message(InnerMessage.ObjectAddRequest)]
    [ResponseType(nameof(ObjectAddResponse))]
    public partial class ObjectAddRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        [MemoryPackOrder(3)]
        public ActorId ActorId { get; set; }

        public static ObjectAddRequest Create(int type = default, long key = default, ActorId actorId = default, bool isFromPool = false)
        {
            ObjectAddRequest msg = ObjectPool.Instance.Fetch(typeof(ObjectAddRequest), isFromPool) as ObjectAddRequest;
            msg.Set(type, key, actorId);
            return msg;
        }

        public void Set(int type = default, long key = default, ActorId actorId = default)
        {
            this.Type = type;
            this.Key = key;
            this.ActorId = actorId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectAddResponse)]
    public partial class ObjectAddResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static ObjectAddResponse Create(int error = default, string message = default, bool isFromPool = false)
        {
            ObjectAddResponse msg = ObjectPool.Instance.Fetch(typeof(ObjectAddResponse), isFromPool) as ObjectAddResponse;
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
    [Message(InnerMessage.ObjectLockRequest)]
    [ResponseType(nameof(ObjectLockResponse))]
    public partial class ObjectLockRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        [MemoryPackOrder(3)]
        public ActorId ActorId { get; set; }

        [MemoryPackOrder(4)]
        public int Time { get; set; }

        public static ObjectLockRequest Create(int type = default, long key = default, ActorId actorId = default, int time = default, bool isFromPool = false)
        {
            ObjectLockRequest msg = ObjectPool.Instance.Fetch(typeof(ObjectLockRequest), isFromPool) as ObjectLockRequest;
            msg.Set(type, key, actorId, time);
            return msg;
        }

        public void Set(int type = default, long key = default, ActorId actorId = default, int time = default)
        {
            this.Type = type;
            this.Key = key;
            this.ActorId = actorId;
            this.Time = time;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;
            this.ActorId = default;
            this.Time = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectLockResponse)]
    public partial class ObjectLockResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static ObjectLockResponse Create(int error = default, string message = default, bool isFromPool = false)
        {
            ObjectLockResponse msg = ObjectPool.Instance.Fetch(typeof(ObjectLockResponse), isFromPool) as ObjectLockResponse;
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
    [Message(InnerMessage.ObjectUnLockRequest)]
    [ResponseType(nameof(ObjectUnLockResponse))]
    public partial class ObjectUnLockRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        [MemoryPackOrder(3)]
        public ActorId OldActorId { get; set; }

        [MemoryPackOrder(4)]
        public ActorId NewActorId { get; set; }

        public static ObjectUnLockRequest Create(int type = default, long key = default, ActorId oldActorId = default, ActorId newActorId = default, bool isFromPool = false)
        {
            ObjectUnLockRequest msg = ObjectPool.Instance.Fetch(typeof(ObjectUnLockRequest), isFromPool) as ObjectUnLockRequest;
            msg.Set(type, key, oldActorId, newActorId);
            return msg;
        }

        public void Set(int type = default, long key = default, ActorId oldActorId = default, ActorId newActorId = default)
        {
            this.Type = type;
            this.Key = key;
            this.OldActorId = oldActorId;
            this.NewActorId = newActorId;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;
            this.OldActorId = default;
            this.NewActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectUnLockResponse)]
    public partial class ObjectUnLockResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static ObjectUnLockResponse Create(int error = default, string message = default, bool isFromPool = false)
        {
            ObjectUnLockResponse msg = ObjectPool.Instance.Fetch(typeof(ObjectUnLockResponse), isFromPool) as ObjectUnLockResponse;
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
    [Message(InnerMessage.ObjectRemoveRequest)]
    [ResponseType(nameof(ObjectRemoveResponse))]
    public partial class ObjectRemoveRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        public static ObjectRemoveRequest Create(int type = default, long key = default, bool isFromPool = false)
        {
            ObjectRemoveRequest msg = ObjectPool.Instance.Fetch(typeof(ObjectRemoveRequest), isFromPool) as ObjectRemoveRequest;
            msg.Set(type, key);
            return msg;
        }

        public void Set(int type = default, long key = default)
        {
            this.Type = type;
            this.Key = key;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectRemoveResponse)]
    public partial class ObjectRemoveResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static ObjectRemoveResponse Create(int error = default, string message = default, bool isFromPool = false)
        {
            ObjectRemoveResponse msg = ObjectPool.Instance.Fetch(typeof(ObjectRemoveResponse), isFromPool) as ObjectRemoveResponse;
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
    [Message(InnerMessage.ObjectGetRequest)]
    [ResponseType(nameof(ObjectGetResponse))]
    public partial class ObjectGetRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        public static ObjectGetRequest Create(int type = default, long key = default, bool isFromPool = false)
        {
            ObjectGetRequest msg = ObjectPool.Instance.Fetch(typeof(ObjectGetRequest), isFromPool) as ObjectGetRequest;
            msg.Set(type, key);
            return msg;
        }

        public void Set(int type = default, long key = default)
        {
            this.Type = type;
            this.Key = key;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectGetResponse)]
    public partial class ObjectGetResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public int Type { get; set; }

        [MemoryPackOrder(4)]
        public ActorId ActorId { get; set; }

        public static ObjectGetResponse Create(int error = default, string message = default, int type = default, ActorId actorId = default, bool isFromPool = false)
        {
            ObjectGetResponse msg = ObjectPool.Instance.Fetch(typeof(ObjectGetResponse), isFromPool) as ObjectGetResponse;
            msg.Set(error, message, type, actorId);
            return msg;
        }

        public void Set(int error = default, string message = default, int type = default, ActorId actorId = default)
        {
            this.Error = error;
            this.Message = message;
            this.Type = type;
            this.ActorId = actorId;
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
            this.Type = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.R2G_GetLoginKey)]
    [ResponseType(nameof(G2R_GetLoginKey))]
    public partial class R2G_GetLoginKey : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Account { get; set; }

        public static R2G_GetLoginKey Create(string account = default, bool isFromPool = false)
        {
            R2G_GetLoginKey msg = ObjectPool.Instance.Fetch(typeof(R2G_GetLoginKey), isFromPool) as R2G_GetLoginKey;
            msg.Set(account);
            return msg;
        }

        public void Set(string account = default)
        {
            this.Account = account;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Account = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2R_GetLoginKey)]
    public partial class G2R_GetLoginKey : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long Key { get; set; }

        [MemoryPackOrder(4)]
        public long GateId { get; set; }

        public static G2R_GetLoginKey Create(int error = default, string message = default, long key = default, long gateId = default, bool isFromPool = false)
        {
            G2R_GetLoginKey msg = ObjectPool.Instance.Fetch(typeof(G2R_GetLoginKey), isFromPool) as G2R_GetLoginKey;
            msg.Set(error, message, key, gateId);
            return msg;
        }

        public void Set(int error = default, string message = default, long key = default, long gateId = default)
        {
            this.Error = error;
            this.Message = message;
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
            this.Key = default;
            this.GateId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2M_SessionDisconnect)]
    public partial class G2M_SessionDisconnect : MessageObject, ILocationMessage
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public static G2M_SessionDisconnect Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2M_SessionDisconnect), isFromPool) as G2M_SessionDisconnect;
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
    [Message(InnerMessage.ObjectQueryResponse)]
    public partial class ObjectQueryResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public byte[] Entity { get; set; }

        public static ObjectQueryResponse Create(int error = default, string message = default, byte[] entity = default, bool isFromPool = false)
        {
            ObjectQueryResponse msg = ObjectPool.Instance.Fetch(typeof(ObjectQueryResponse), isFromPool) as ObjectQueryResponse;
            msg.Set(error, message, entity);
            return msg;
        }

        public void Set(int error = default, string message = default, byte[] entity = default)
        {
            this.Error = error;
            this.Message = message;
            this.Entity = entity;
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
            this.Entity = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2M_UnitTransferRequest)]
    [ResponseType(nameof(M2M_UnitTransferResponse))]
    public partial class M2M_UnitTransferRequest : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public ActorId OldActorId { get; set; }

        [MemoryPackOrder(2)]
        public byte[] Unit { get; set; }

        [MemoryPackOrder(3)]
        public List<byte[]> Entitys { get; set; } = new();

        public static M2M_UnitTransferRequest Create(ActorId oldActorId = default, byte[] unit = default, bool isFromPool = false)
        {
            M2M_UnitTransferRequest msg = ObjectPool.Instance.Fetch(typeof(M2M_UnitTransferRequest), isFromPool) as M2M_UnitTransferRequest;
            msg.Set(oldActorId, unit);
            return msg;
        }

        public void Set(ActorId oldActorId = default, byte[] unit = default)
        {
            this.OldActorId = oldActorId;
            this.Unit = unit;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.OldActorId = default;
            this.Unit = default;
            this.Entitys.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2M_UnitTransferResponse)]
    public partial class M2M_UnitTransferResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public static M2M_UnitTransferResponse Create(int error = default, string message = default, bool isFromPool = false)
        {
            M2M_UnitTransferResponse msg = ObjectPool.Instance.Fetch(typeof(M2M_UnitTransferResponse), isFromPool) as M2M_UnitTransferResponse;
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

    public static class InnerMessage
    {
        public const ushort ObjectQueryRequest = 20002;
        public const ushort M2A_Reload = 20003;
        public const ushort A2M_Reload = 20004;
        public const ushort G2G_LockRequest = 20005;
        public const ushort G2G_LockResponse = 20006;
        public const ushort G2G_LockReleaseRequest = 20007;
        public const ushort G2G_LockReleaseResponse = 20008;
        public const ushort ObjectAddRequest = 20009;
        public const ushort ObjectAddResponse = 20010;
        public const ushort ObjectLockRequest = 20011;
        public const ushort ObjectLockResponse = 20012;
        public const ushort ObjectUnLockRequest = 20013;
        public const ushort ObjectUnLockResponse = 20014;
        public const ushort ObjectRemoveRequest = 20015;
        public const ushort ObjectRemoveResponse = 20016;
        public const ushort ObjectGetRequest = 20017;
        public const ushort ObjectGetResponse = 20018;
        public const ushort R2G_GetLoginKey = 20019;
        public const ushort G2R_GetLoginKey = 20020;
        public const ushort G2M_SessionDisconnect = 20021;
        public const ushort ObjectQueryResponse = 20022;
        public const ushort M2M_UnitTransferRequest = 20023;
        public const ushort M2M_UnitTransferResponse = 20024;
    }
}