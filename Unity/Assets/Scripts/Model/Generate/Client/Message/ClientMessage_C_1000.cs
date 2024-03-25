using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(ClientMessage.Main2NetClient_Login)]
    [ResponseType(nameof(NetClient2Main_Login))]
    public partial class Main2NetClient_Login : MessageObject, IRequest
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int OwnerFiberId { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [MemoryPackOrder(2)]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MemoryPackOrder(3)]
        public string Password { get; set; }

        /// <summary>
        /// Create Main2NetClient_Login
        /// </summary>
        /// <param name="ownerFiberId">OwnerFiberId</param>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        /// <param name="isFromPool"></param>
        public static Main2NetClient_Login Create(int ownerFiberId = default, string account = default, string password = default, bool isFromPool = false)
        {
            Main2NetClient_Login msg = ObjectPool.Instance.Fetch(typeof(Main2NetClient_Login), isFromPool) as Main2NetClient_Login;
            msg.Set(ownerFiberId, account, password);
            return msg;
        }

        /// <summary>
        /// Set Main2NetClient_Login
        /// </summary>
        /// <param name="ownerFiberId">OwnerFiberId</param>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        public void Set(int ownerFiberId = default, string account = default, string password = default)
        {
            this.OwnerFiberId = ownerFiberId;
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
            this.OwnerFiberId = default;
            this.Account = default;
            this.Password = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(ClientMessage.NetClient2Main_Login)]
    public partial class NetClient2Main_Login : MessageObject, IResponse
    {
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long PlayerId { get; set; }

        public static NetClient2Main_Login Create(int error = default, string message = default, long playerId = default, bool isFromPool = false)
        {
            NetClient2Main_Login msg = ObjectPool.Instance.Fetch(typeof(NetClient2Main_Login), isFromPool) as NetClient2Main_Login;
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

    public static class ClientMessage
    {
        public const ushort Main2NetClient_Login = 1001;
        public const ushort NetClient2Main_Login = 1002;
    }
}