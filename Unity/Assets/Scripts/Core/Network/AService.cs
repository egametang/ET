using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public Action<long, IPEndPoint> AcceptCallback;
        public Action<long, ActorId, object> ReadCallback;
        public Action<long, int> ErrorCallback;
        
        public long Id { get; set; }
        
        public ServiceType ServiceType { get; protected set; }
        
        public const int MaxCacheBufferSize = 1024;
		
        private readonly Queue<MemoryBuffer> pool = new();

        public MemoryBuffer Fetch(int size = 0)
        {
            if (size > MaxCacheBufferSize)
            {
                return new MemoryBuffer(size);
            }
            
            if (size < MaxCacheBufferSize)
            {
                size = MaxCacheBufferSize;
            }
            
            if (this.pool.Count == 0)
            {
                return new MemoryBuffer(size);
            }
            
            return pool.Dequeue();
        }

        public void Recycle(MemoryBuffer memoryBuffer)
        {
            if (memoryBuffer.Capacity > 1024)
            {
                return;
            }
            
            if (this.pool.Count > 10) // 这里不需要太大，其实Kcp跟Tcp,这里1就足够了
            {
                return;
            }
            
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);
            
            this.pool.Enqueue(memoryBuffer);
        }
        
        public AService()
        {
            NetServices.Instance.Add(this);
        }
        
        public virtual void Dispose()
        {
            NetServices.Instance?.Remove(this.Id);
        }

        public abstract void Update();

        public abstract void Remove(long id, int error = 0);
        
        public abstract bool IsDisposed();

        public abstract void Create(long id, IPEndPoint address);

        public abstract void Send(long channelId, ActorId actorId, MessageObject message);

        public virtual (uint, uint) GetChannelConn(long channelId)
        {
            throw new Exception($"default conn throw Exception! {channelId}");
        }
        
        public virtual void ChangeAddress(long channelId, IPEndPoint ipEndPoint)
        {
        }
    }
}