using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ET
{
    public enum NetworkProtocol
    {
        TCP,
        KCP,
        Websocket,
    }

    public class NetServices: Singleton<NetServices>
    {
        private readonly ConcurrentDictionary<long, AService> services = new();
        
        private readonly ConcurrentQueue<MemoryBuffer> pool = new();

        private long idGenerator;

        public override void Dispose()
        {
            if (this.IsDisposed())
            {
                return;
            }
            
            base.Dispose();
            
            foreach (var kv in this.services)
            {
                kv.Value.Dispose();
            }
        }
     
        public MemoryBuffer FetchMemoryBuffer()
        {
            MemoryBuffer memoryBuffer;
            if (this.pool.TryDequeue(out memoryBuffer))
            {
                return memoryBuffer;
            }

            memoryBuffer = new(128) { IsFromPool = true };
            return memoryBuffer;
        }

        public void RecycleMemoryBuffer(MemoryBuffer memoryBuffer)
        {
            if (memoryBuffer == null)
            {
                return;
            }
            
            if (!memoryBuffer.IsFromPool)
            {
                return;
            }
            if (memoryBuffer.Capacity > 128) // 太大的不回收，GC
            {
                return;
            }

            if (this.pool.Count > 1000)
            {
                return;
            }

            memoryBuffer.SetLength(0);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            this.pool.Enqueue(memoryBuffer);
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