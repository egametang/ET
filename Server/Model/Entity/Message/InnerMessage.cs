using System.Collections.Generic;using MongoDB.Bson.Serialization.Attributes;// 服务器内部消息 Opcode从10000开始namespace Model{	public abstract partial class AMessage	{	}	public abstract partial class ARequest : AMessage	{	}	public abstract partial class AResponse : AMessage	{	}	public abstract partial class AActorMessage : AMessage	{	}		public abstract partial class AActorRequest : ARequest	{	}		public abstract partial class AActorResponse : AResponse	{	}

    #region 原有消息


    /// <summary>    /// 用来包装actor消息    /// </summary>    [Message(Opcode.ActorRequest)]
    [BsonIgnoreExtraElements]
    public class ActorRequest : ARequest
    {
        public long Id { get; set; }

        public AMessage AMessage { get; set; }
    }













    /// <summary>    /// actor RPC消息响应    /// </summary>    [Message(Opcode.ActorResponse)]
    [BsonIgnoreExtraElements]
    public class ActorResponse : AResponse
    {
    }













    /// <summary>    /// 用来包装actor消息    /// </summary>    [Message(Opcode.ActorRpcRequest)]
    [BsonIgnoreExtraElements]
    public class ActorRpcRequest : ActorRequest
    {
    }













    /// <summary>    /// actor RPC消息响应带回应    /// </summary>    [Message(Opcode.ActorRpcResponse)]
    [BsonIgnoreExtraElements]
    public class ActorRpcResponse : ActorResponse
    {
        public AMessage AMessage { get; set; }
    }














    /// <summary>    /// 传送unit    /// </summary>    [Message(Opcode.M2M_TrasferUnitRequest)]
    [BsonIgnoreExtraElements]
    public class M2M_TrasferUnitRequest : ARequest
    {
        public Unit Unit;
    }

    [Message(Opcode.M2M_TrasferUnitResponse)]
    [BsonIgnoreExtraElements]
    public class M2M_TrasferUnitResponse : AResponse
    {
    }



    [Message(Opcode.M2A_Reload)]
    [BsonIgnoreExtraElements]
    public class M2A_Reload : ARequest
    {
    }


    [Message(Opcode.A2M_Reload)]
    [BsonIgnoreExtraElements]
    public class A2M_Reload : AResponse
    {
    }


    [Message(Opcode.G2G_LockRequest)]
    [BsonIgnoreExtraElements]
    public class G2G_LockRequest : ARequest
    {
        public long Id;
        public string Address;
    }


    [Message(Opcode.G2G_LockResponse)]
    [BsonIgnoreExtraElements]
    public class G2G_LockResponse : AResponse
    {
    }

    [Message(Opcode.G2G_LockReleaseRequest)]
    [BsonIgnoreExtraElements]
    public class G2G_LockReleaseRequest : ARequest
    {
        public long Id;
        public string Address;
    }


    [Message(Opcode.G2G_LockReleaseResponse)]
    [BsonIgnoreExtraElements]
    public class G2G_LockReleaseResponse : AResponse
    {
    }

    [Message(Opcode.DBSaveRequest)]
    [BsonIgnoreExtraElements]
    public class DBSaveRequest : ARequest
    {
        public bool NeedCache = true;

        public string CollectionName = "";

        public Disposer Disposer;
    }



    [Message(Opcode.DBSaveBatchResponse)]
    [BsonIgnoreExtraElements]
    public class DBSaveBatchResponse : AResponse
    {
    }


    [Message(Opcode.DBSaveBatchRequest)]
    [BsonIgnoreExtraElements]
    public class DBSaveBatchRequest : ARequest
    {
        public bool NeedCache = true;
        public string CollectionName = "";
        public List<Disposer> Disposers = new List<Disposer>();
    }

    [Message(Opcode.DBSaveResponse)]
    [BsonIgnoreExtraElements]
    public class DBSaveResponse : AResponse
    {
    }


    [Message(Opcode.DBQueryRequest)]
    [BsonIgnoreExtraElements]
    public class DBQueryRequest : ARequest
    {
        public long Id;
        public string CollectionName { get; set; }
        public bool NeedCache = true;
    }


    [Message(Opcode.DBQueryResponse)]
    [BsonIgnoreExtraElements]
    public class DBQueryResponse : AResponse
    {
        public Disposer Disposer;
    }


    [Message(Opcode.DBQueryBatchRequest)]
    [BsonIgnoreExtraElements]
    public class DBQueryBatchRequest : ARequest
    {
        public string CollectionName { get; set; }
        public List<long> IdList { get; set; }
        public bool NeedCache = true;
    }

    [Message(Opcode.DBQueryBatchResponse)]
    [BsonIgnoreExtraElements]
    public class DBQueryBatchResponse : AResponse
    {
        public List<Disposer> Disposers;
    }


    [Message(Opcode.DBQueryJsonRequest)]
    [BsonIgnoreExtraElements]
    public class DBQueryJsonRequest : ARequest
    {
        public string CollectionName { get; set; }
        public string Json { get; set; }
        public bool NeedCache = true;
    }

    [Message(Opcode.DBQueryJsonResponse)]
    [BsonIgnoreExtraElements]
    public class DBQueryJsonResponse : AResponse
    {
        public List<Disposer> Disposers;
    }

    [Message(Opcode.ObjectAddRequest)]
    [BsonIgnoreExtraElements]
    public class ObjectAddRequest : ARequest
    {
        public long Key { get; set; }
        public int AppId { get; set; }
    }

    [Message(Opcode.ObjectAddResponse)]
    [BsonIgnoreExtraElements]
    public class ObjectAddResponse : AResponse
    {
    }

    [Message(Opcode.ObjectRemoveRequest)]
    [BsonIgnoreExtraElements]
    public class ObjectRemoveRequest : ARequest
    {
        public long Key { get; set; }
    }

    [Message(Opcode.ObjectRemoveResponse)]
    [BsonIgnoreExtraElements]
    public class ObjectRemoveResponse : AResponse
    {
    }

    [Message(Opcode.ObjectLockRequest)]
    [BsonIgnoreExtraElements]
    public class ObjectLockRequest : ARequest
    {
        public long Key { get; set; }
        public int LockAppId { get; set; }
        public int Time { get; set; }
    }

    [Message(Opcode.ObjectLockResponse)]
    [BsonIgnoreExtraElements]
    public class ObjectLockResponse : AResponse
    {
    }

    [Message(Opcode.ObjectUnLockRequest)]
    [BsonIgnoreExtraElements]
    public class ObjectUnLockRequest : ARequest
    {
        public long Key { get; set; }
        public int UnLockAppId { get; set; }
        public int AppId { get; set; }
    }

    [Message(Opcode.ObjectUnLockResponse)]
    [BsonIgnoreExtraElements]
    public class ObjectUnLockResponse : AResponse
    {
    }

    [Message(Opcode.ObjectGetRequest)]
    [BsonIgnoreExtraElements]
    public class ObjectGetRequest : ARequest
    {
        public long Key { get; set; }
    }

    [Message(Opcode.ObjectGetResponse)]
    [BsonIgnoreExtraElements]
    public class ObjectGetResponse : AResponse
    {
        public int AppId { get; set; }
    }


    [Message(Opcode.R2G_GetLoginKey)]
    [BsonIgnoreExtraElements]
    public class R2G_GetLoginKey : ARequest
    {
        public long Account;
    }


    [Message(Opcode.G2R_GetLoginKey)]
    [BsonIgnoreExtraElements]
    public class G2R_GetLoginKey : AResponse
    {
        public long Key;

        public G2R_GetLoginKey()
        {
        }

        public G2R_GetLoginKey(long key)
        {
            this.Key = key;
        }
    }


    [Message(Opcode.G2M_CreateUnit)]
    [BsonIgnoreExtraElements]
    public class G2M_CreateUnit : ARequest
    {
        public long PlayerId;
        public long GateSessionId;
    }


    [Message(Opcode.M2G_CreateUnit)]
    [BsonIgnoreExtraElements]
    public class M2G_CreateUnit : AResponse
    {
        public long UnitId;
        public int Count;
    }


    #endregion
    #region Gate
    /// <summary>
    /// 获取授权Key
    /// </summary>
    [Message(Opcode.GetLoginKeyRt)]    [BsonIgnoreExtraElements]    public class GetLoginKeyRt : ARequest    {        public long UserId;    }    /// <summary>    /// 返回授权Key    /// </summary>    [Message(Opcode.GetLoginKeyRe)]    [BsonIgnoreExtraElements]    public class GetLoginKeyRe : AResponse    {        public long Key;    }

    /// <summary>    /// 用户信息 请求    /// </summary>    [Message(Opcode.GetUserInfoRt)]    public class GetUserInfoRt : ARequest    {        public long UserId;    }

    /// <summary>    /// 用户信息 应答    /// </summary>    [Message(Opcode.GetUserInfoRe)]    public class GetUserInfoRe : AResponse    {        public string NickName;        public int Wins;        public int Loses;        public long Money;    }


    /// <summary>    /// 登录授权 请求    /// </summary>    [Message(Opcode.LoginGateRt)]    public class LoginGateRt : ARequest    {        public long Key;    }


    /// <summary>    /// 登录授权 应答    /// </summary>    [Message(Opcode.LoginGateRe)]    public class LoginGateRe : AResponse    {        public long PlayerId;        public long UserId;    }    #endregion

    #region 网关消息
    /// <summary>    /// 用户断开    /// </summary>    [Message(Opcode.PlayerDisconnect)]    [BsonIgnoreExtraElements]    public class PlayerDisconnect : AMessage    {        public long UserId;    }    /// <summary>
    /// 用户退出
    /// </summary>    [Message(Opcode.PlayerQuit)]    [BsonIgnoreExtraElements]    public class PlayerQuit : AActorMessage    {        public long PlayerId;    }    /// <summary>
    /// 加入匹配
    /// </summary>    [Message(Opcode.JoinMatchRt)]    [BsonIgnoreExtraElements]    public class JoinMatchRt : ARequest    {        public long PlayerId;        public long UserId;        public long GateSessionId;        public int GateAppId;    }    /// <summary>
    /// 匹配反馈
    /// </summary>    [Message(Opcode.JoinMatchRe)]    [BsonIgnoreExtraElements]    public class JoinMatchRe : AResponse    {        public long ActorId;    }

    #endregion
    #region Match
    /// <summary>
    /// 游戏匹配成功
    /// </summary>    [Message(Opcode.MatchSuccess)]    [BsonIgnoreExtraElements]    public class MatchSuccess : AMessage    {        public long PlayerId;        public long RoomId;        public long Key;    }    #endregion

    #region DDZ
    /// <summary>    /// 用户退出房间    /// </summary>    [Message(Opcode.GamerQuitRoom)]    [BsonIgnoreExtraElements]    public class GamerQuitRoom : AMessage    {        public long RoomId;        public long PlayerId;    }    [Message(Opcode.CreateRoomRt)]    [BsonIgnoreExtraElements]    public class CreateRoomRt : ARequest    {        public RoomLevel Level;    }    [Message(Opcode.CreateRoomRe)]    [BsonIgnoreExtraElements]    public class CreateRoomRe : AResponse    {        public long RoomId;    }    #endregion

}