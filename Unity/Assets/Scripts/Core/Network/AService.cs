using System;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public int Id { get; set; }
        
        public ServiceType ServiceType { get; protected set; }
        
        public ThreadSynchronizationContext ThreadSynchronizationContext;
        
        protected AService()
        {
            NetServices.Instance.Add(this);
        }

        public virtual void Dispose()
        {
            NetServices.Instance.Remove(this);
        }
        
        // localConn放在低32bit
        private long connectIdGenerater = int.MaxValue;
        public long CreateConnectChannelId(uint localConn)
        {
            return (--this.connectIdGenerater << 32) | localConn;
        }
        
        public uint CreateRandomLocalConn()
        {
            return (1u << 30) | RandomGenerator.Instance.RandUInt32();
        }

        // localConn放在低32bit
        private long acceptIdGenerater = 1;
        public long CreateAcceptChannelId(uint localConn)
        {
            return (++this.acceptIdGenerater << 32) | localConn;
        }


        public abstract void Update();

        public abstract void Remove(long id);
        
        public abstract bool IsDispose();

        protected abstract void Get(long id, IPEndPoint address);

        protected abstract void Send(long channelId, long actorId, MemoryStream stream);
        
        protected void OnAccept(long channelId, IPEndPoint ipEndPoint)
        {
            this.AcceptCallback.Invoke(channelId, ipEndPoint);
        }

        public void OnRead(long channelId, MemoryStream memoryStream)
        {
            this.ReadCallback.Invoke(channelId, memoryStream);
        }

        public void OnError(long channelId, int e)
        {
            this.Remove(channelId);
            
            this.ErrorCallback?.Invoke(channelId, e);
        }

        
        public Action<long, IPEndPoint> AcceptCallback;
        public Action<long, int> ErrorCallback;
        public Action<long, MemoryStream> ReadCallback;

        public void RemoveChannel(long channelId)
        {
            this.Remove(channelId);
        }

        public void SendStream(long channelId, long actorId, MemoryStream stream)
        {
            this.Send(channelId, actorId, stream);
        }

        public void GetOrCreate(long id, IPEndPoint address)
        {
            this.Get(id, address);
        }
    }
}