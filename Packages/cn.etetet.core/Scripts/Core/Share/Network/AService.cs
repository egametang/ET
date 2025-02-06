using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public Action<long, IPEndPoint> AcceptCallback;
        public Action<long, MemoryBuffer> ReadCallback;
        public Action<long, int> ErrorCallback;

        public long Id { get; set; } = IdGenerater.Instance.GenerateId();
        
        public ServiceType ServiceType { get; protected set; }
        
        private const int MaxMemoryBufferSize = 1024;
		
        private readonly Queue<MemoryBuffer> pool = new();

        public MemoryBuffer Fetch(int size = 0)
        {
            if (size > MaxMemoryBufferSize)
            {
                return new MemoryBuffer(size);
            }
            
            if (size < MaxMemoryBufferSize)
            {
                size = MaxMemoryBufferSize;
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
        
        
        public virtual void Dispose()
        {
            this.Id = 0;
        }

        public abstract void Update();

        public abstract void Remove(long id, int error = 0);

        public bool IsDisposed()
        {
            return this.Id == 0;
        }
        
        public abstract void Create(long id, IPEndPoint ipEndPoint);

        public abstract void Send(long channelId, MemoryBuffer memoryBuffer);

        public virtual (uint, uint) GetChannelConn(long channelId)
        {
            throw new Exception($"default conn throw Exception! {channelId}");
        }
        
        public virtual void ChangeAddress(long channelId, IPEndPoint ipEndPoint)
        {
        }
    }
}