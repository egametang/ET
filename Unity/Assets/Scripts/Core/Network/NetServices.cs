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
    }

    public class NetServices: Singleton<NetServices>, ISingletonAwake
    {
        private readonly ConcurrentDictionary<long, AService> services = new();
        
        private long idGenerator;
        
        public void Awake()
        {
        }

        protected override void Destroy()
        {
            foreach (var kv in this.services)
            {
                kv.Value.Dispose();
            }
        }

        public void Add(AService aService)
        {
            aService.Id = Interlocked.Increment(ref this.idGenerator);
            this.services[aService.Id] = aService;
        }

        public AService Get(long id)
        {
            AService aService;
            this.services.TryGetValue(id, out aService);
            return aService;
        }

        public void Remove(long id)
        {
            this.services.Remove(id, out AService _);
        }

        // 这个因为是NetClientComponent中使用，不会与Accept冲突
        public uint CreateConnectChannelId()
        {
            return RandomGenerator.RandUInt32();
        }

        // 防止与内网进程号的ChannelId冲突，所以设置为一个大的随机数
        private uint acceptIdGenerator = uint.MaxValue;

        public uint CreateAcceptChannelId()
        {
            return --this.acceptIdGenerator;
        }
        
    }
}