using System;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public ServiceType ServiceType { get; protected set; }
        
        public ThreadSynchronizationContext ThreadSynchronizationContext;
        
        // localConn放在低32bit
        private long connectIdGenerater = int.MaxValue;
        public long CreateConnectChannelId(uint localConn)
        {
            return (--this.connectIdGenerater << 32) | localConn;
        }
        
        public uint CreateRandomLocalConn(Random random)
        {
            return (1u << 30) | random.RandUInt32();
        }

#region 网络线程
        
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

        public abstract void Dispose();

        protected abstract void Send(long channelId, long actorId, MemoryStream stream);
        
        protected void OnAccept(long channelId, IPEndPoint ipEndPoint)
        {
#if NET_THREAD
            ThreadSynchronizationContext.Instance.Post(() =>
            {
                this.AcceptCallback.Invoke(channelId, ipEndPoint);
            });
#else
            this.AcceptCallback.Invoke(channelId, ipEndPoint);
#endif
        }

        public void OnRead(long channelId, MemoryStream memoryStream)
        {
#if NET_THREAD
            ThreadSynchronizationContext.Instance.Post(() =>
            {
                this.ReadCallback.Invoke(channelId, memoryStream);
            });
#else
            this.ReadCallback.Invoke(channelId, memoryStream);
#endif
        }

        public void OnError(long channelId, int e)
        {
            this.Remove(channelId);
            
#if NET_THREAD
            ThreadSynchronizationContext.Instance.Post(() =>
            {
                this.ErrorCallback?.Invoke(channelId, e);
            });
#else
            this.ErrorCallback?.Invoke(channelId, e);
#endif
        }

#endregion


#region 主线程
        
        public Action<long, IPEndPoint> AcceptCallback;
        public Action<long, int> ErrorCallback;
        public Action<long, MemoryStream> ReadCallback;

        public void Destroy()
        {
#if NET_THREAD
            this.ThreadSynchronizationContext.Post(this.Dispose);
#else
            this.Dispose();
#endif
        }

        public void RemoveChannel(long channelId)
        {
#if NET_THREAD
            this.ThreadSynchronizationContext.Post(() =>
            {
                this.Remove(channelId);
            });
#else
            this.Remove(channelId);
#endif
        }

        public void SendStream(long channelId, long actorId, MemoryStream stream)
        {
#if NET_THREAD
            this.ThreadSynchronizationContext.Post(() =>
            {
                this.Send(channelId, actorId, stream);
            });
#else
            this.Send(channelId, actorId, stream);
#endif
        }

        public void GetOrCreate(long id, IPEndPoint address)
        {
#if NET_THREAD
            this.ThreadSynchronizationContext.Post(()=>
            {
                this.Get(id, address);
            });
#else
            this.Get(id, address);
#endif
        }

#endregion

    }
}