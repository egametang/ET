using System;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public int Id { get; set; }
        
        public ServiceType ServiceType { get; protected set; }
        
        private (object Message, MemoryBuffer MemoryStream) lastMessageInfo;
        
        // 缓存上一个发送的消息，这样广播消息的时候省掉多次序列化,这样有个另外的问题,客户端就不能保存发送的消息来减少gc
        public MemoryBuffer Fetch(object message)
        {
            if (object.ReferenceEquals(lastMessageInfo.Message, message))
            {
                Log.Debug($"message serialize cache: {message.GetType().Name}");
                return lastMessageInfo.MemoryStream;
            }

            MemoryBuffer stream = NetServices.Instance.Fetch();
            MessageSerializeHelper.MessageToStream(stream, message);
            this.lastMessageInfo = (message, stream);
            return stream;
        }

        private MemoryBuffer lastRecyleMessage;
        
        // Recycle不能直接回到池中，因为有可能广播消息的时候，发送的是同一个MemoryBuff
        public void Recycle(MemoryBuffer memoryStream)
        {
            if (this.lastRecyleMessage == null)
            {
                this.lastRecyleMessage = memoryStream;
                return;
            }

            if (ReferenceEquals(this.lastRecyleMessage, memoryStream))
            {
                return;
            }
            
            NetServices.Instance.Recycle(this.lastRecyleMessage);
            this.lastRecyleMessage = memoryStream;
        }
        
        public virtual void Dispose()
        {
        }

        public abstract void Update();

        public abstract void Remove(long id, int error = 0);
        
        public abstract bool IsDispose();

        public abstract void Create(long id, IPEndPoint address);

        public abstract void Send(long channelId, long actorId, object message);

        public virtual (uint, uint) GetChannelConn(long channelId)
        {
            throw new Exception($"default conn throw Exception! {channelId}");
        }
        
        public virtual void ChangeAddress(long channelId, IPEndPoint ipEndPoint)
        {
        }
    }
}