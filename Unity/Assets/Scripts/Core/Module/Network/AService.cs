using System;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public int Id { get; set; }
        
        public ServiceType ServiceType { get; protected set; }
        
        public virtual void Dispose()
        {
        }

        public abstract void Update();

        public abstract void Remove(long id);
        
        public abstract bool IsDispose();

        public abstract void Create(long id, IPEndPoint address);

        public abstract void Send(long channelId, long actorId, MemoryStream stream);
    }
}