using System;
using System.Collections.Generic;
using System.IO;
using System.Net;



namespace ET
{
    public enum NetworkProtocol
    {
        TCP,
        KCP,
        Websocket,
    }

    [ComponentOf(typeof(Fiber))]
    public class NetServices: Entity, IAwake
    {
        private readonly Dictionary<int, Action<long, IPEndPoint>> acceptCallback = new();
        private readonly Dictionary<int, Action<long, ActorId, object>> readCallback = new();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new();
        
        private readonly Dictionary<int, AService> services = new();
        private readonly Queue<int> queue = new();

        private readonly Queue<MemoryBuffer> pool = new();

        private int serviceIdGenerator;

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();

            foreach (var kv in this.services)
            {
                kv.Value.Dispose();
            }
        }

        public (uint, uint) GetChannelConn(int serviceId, long channelId)
        {
            AService service = this.Get(serviceId);
            if (service == null)
            {
                return (0, 0);
            }

            return service.GetChannelConn(channelId);
        }

        public void ChangeAddress(int serviceId, long channelId, IPEndPoint ipEndPoint)
        {
            AService service = this.Get(serviceId);
            if (service == null)
            {
                return;
            }

            service.ChangeAddress(channelId, ipEndPoint);
        }

        public void SendMessage(int serviceId, long channelId, ActorId actorId, MessageObject message)
        {
            AService service = this.Get(serviceId);
            if (service != null)
            {
                service.Send(channelId, actorId, message);
            }
        }

        public int AddService(AService aService)
        {
            aService.Id = ++this.serviceIdGenerator;
            this.Add(aService);
            return aService.Id;
        }

        public void RemoveService(int serviceId)
        {
            this.Remove(serviceId);
        }

        public void RemoveChannel(int serviceId, long channelId, int error)
        {
            AService service = this.Get(serviceId);
            if (service != null)
            {
                service.Remove(channelId, error);
            }
        }

        public void CreateChannel(int serviceId, long channelId, IPEndPoint address)
        {
            AService service = this.Get(serviceId);
            if (service != null)
            {
                service.Create(channelId, address);
            }
        }

        public void RegisterAcceptCallback(int serviceId, Action<long, IPEndPoint> action)
        {
            this.acceptCallback.Add(serviceId, action);
        }

        public void RegisterReadCallback(int serviceId, Action<long, ActorId, object> action)
        {
            this.readCallback.Add(serviceId, action);
        }

        public void RegisterErrorCallback(int serviceId, Action<long, int> action)
        {
            this.errorCallback.Add(serviceId, action);
        }
        

        public MemoryBuffer FetchMemoryBuffer()
        {
            if (this.pool.Count > 0)
            {
                return this.pool.Dequeue();
            }

            MemoryBuffer memoryBuffer = new(128) { IsFromPool = true };
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

        private void Add(AService aService)
        {
            this.services[aService.Id] = aService;
            this.queue.Enqueue(aService.Id);
        }

        public AService Get(int id)
        {
            AService aService;
            this.services.TryGetValue(id, out aService);
            return aService;
        }

        private void Remove(int id)
        {
            if (this.services.Remove(id, out AService service))
            {
                service.Dispose();
            }
        }

        public void Update()
        {
            int count = this.queue.Count;
            while (count-- > 0)
            {
                int serviceId = this.queue.Dequeue();
                if (!this.services.TryGetValue(serviceId, out AService service))
                {
                    continue;
                }

                this.queue.Enqueue(serviceId);
                service.Update();
            }
        }

        public void OnAccept(int serviceId, long channelId, IPEndPoint ipEndPoint)
        {
            if (!this.acceptCallback.TryGetValue(serviceId, out var action))
            {
                return;
            }

            action.Invoke(channelId, ipEndPoint);
        }

        public void OnRead(int serviceId, long channelId, ActorId actorId, object message)
        {
            if (!this.readCallback.TryGetValue(serviceId, out var action))
            {
                return;
            }

            action.Invoke(channelId, actorId, message);
        }

        public void OnError(int serviceId, long channelId, int error)
        {
            if (!this.errorCallback.TryGetValue(serviceId, out Action<long, int> action))
            {
                return;
            }
            action.Invoke(channelId, error);
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