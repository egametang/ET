using System;
using System.IO;
using System.Net;

namespace ET
{
    public abstract class AService: IDisposable
    {
        public int Id { get; set; }
        
        public ServiceType ServiceType { get; protected set; }
        
        private (object Message, MemoryStream MemoryStream) lastMessageInfo;
        
        // 缓存上一个发送的消息，这样广播消息的时候省掉多次序列化,这样有个另外的问题,客户端就不能保存发送的消息来减少gc，
        // 不过这个问题并不大，因为客户端发送的消息是比较少的，如果实在需要，也可以修改这个方法，把outer的消息过滤掉。
        protected MemoryStream GetMemoryStream(object message)
        {
            if (object.ReferenceEquals(lastMessageInfo.Message, message))
            {
                Log.Debug($"message serialize cache: {message.GetType().Name}");
                return lastMessageInfo.MemoryStream;
            }

            (ushort _, MemoryStream stream) = MessageSerializeHelper.MessageToStream(message);
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