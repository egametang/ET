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
        
        // 缓存上一个发送的消息，这样广播消息的时候省掉多次序列化,这样有个另外的问题,客户端就不能保存发送的消息来减少gc
        // 需要广播的消息不能用对象池，因为用了对象池无法使用ReferenceEquals去判断是否是同一个消息,服务端gc很强，这种消息不用池问题不大
        public MemoryBuffer Fetch(MessageObject message)
        {
            MemoryBuffer stream;
            if (!message.IsFromPool) // 不是从池中来的消息, 服务端需要广播的消息不能用对象池
            {
                if (ReferenceEquals(message, this.lastMessageInfo.Message))
                {
                    Log.Debug($"message serialize cache: {message.GetType().FullName}");
                    return lastMessageInfo.MemoryStream;
                }

                stream = new MemoryBuffer(); // 因为广播，可能MemoryBuffer会用多次，所以不能用对象池
                MessageSerializeHelper.MessageToStream(stream, message);
                this.lastMessageInfo = (message, stream);
            }
            else
            {
                stream = NetServices.Instance.Fetch();
                MessageSerializeHelper.MessageToStream(stream, message);
                NetServices.Instance.RecycleMessage(message);
            }
            return stream;
        }
        
        public void Recycle(MemoryBuffer memoryStream)
        {
            if (!memoryStream.IsFromPool)
            {
                return;
            }
            NetServices.Instance.Recycle(memoryStream);
        }
        
        public virtual void Dispose()
        {
        }

        public abstract void Update();

        public abstract void Remove(long id, int error = 0);
        
        public abstract bool IsDispose();

        public abstract void Create(long id, IPEndPoint address);

        public abstract void Send(long channelId, long actorId, MessageObject message);

        public virtual (uint, uint) GetChannelConn(long channelId)
        {
            throw new Exception($"default conn throw Exception! {channelId}");
        }
        
        public virtual void ChangeAddress(long channelId, IPEndPoint ipEndPoint)
        {
        }
    }
}