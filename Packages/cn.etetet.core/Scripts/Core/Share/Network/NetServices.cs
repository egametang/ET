using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public enum NetworkProtocol
    {
        TCP,
        KCP,
        Websocket,
        UDP,
    }

    public class NetServices: Singleton<NetServices>, ISingletonAwake
    {
        private long idGenerator;
        
        public void Awake()
        {
        }

        // 这个因为是NetClientComponent中使用，不会与Accept冲突
        public uint CreateConnectChannelId()
        {
            return RandomGenerator.RandUInt32();
        }

        // 防止与内网进程号的ChannelId冲突，所以设置为一个大的随机数
        private int acceptIdGenerator = int.MinValue;

        public uint CreateAcceptChannelId()
        {
            return (uint)Interlocked.Add(ref this.acceptIdGenerator, 1);
        }
        
    }
}