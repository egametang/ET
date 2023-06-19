using System;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public int Id { get; set; }
        
        public ServiceType ServiceType { get; protected set; }
        
        private (MessageObject Message, MemoryBuffer MemoryStream) lastMessageInfo;
        
        // 缓存上一个发送的消息，这样广播消息的时候省掉多次序列化
        public MemoryBuffer Fetch(MessageObject message)
        {
            // 这里虽然用了对象池，但是相邻的两个消息不会是池中来的同一个消息，因为lastMessageInfo中的消息还没回收
            if (ReferenceEquals(message, this.lastMessageInfo.Message))
            {
                Log.Debug($"message serialize cache: {message.GetType().FullName}");
                return lastMessageInfo.MemoryStream;
            }
            
            // 回收上一个消息跟MemoryBuffer
            ObjectPool.Instance.Recycle(this.lastMessageInfo.Message);
            NetServices.Instance.RecycleMemoryBuffer(this.lastMessageInfo.MemoryStream);

            MemoryBuffer stream = NetServices.Instance.FetchMemoryBuffer();
            MessageSerializeHelper.MessageToStream(stream, message);
            this.lastMessageInfo = (message, stream);
            return stream;
        }
        
        public virtual void Dispose()
        {
        }

        public abstract void Update();

        public abstract void Remove(long id, int error = 0);
        
        public abstract bool IsDispose();

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