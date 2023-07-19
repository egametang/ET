using System;

namespace ET
{
    public abstract class MessageSessionHandler<Message>: IMessageSessionHandler where Message : MessageObject
    {
        protected abstract ETTask Run(Session session, Message message);

        public void Handle(Session session, object msg)
        {
            HandleAsync(session, msg).Coroutine();
        }

        private async ETTask HandleAsync(Session session, object message)
        {
            using Message msg = message as Message;
            if (message == null)
            {
                Log.Error($"消息类型转换错误: {msg.GetType().FullName} to {typeof (Message).Name}");
                return;
            }

            if (session.IsDisposed)
            {
                Log.Error($"session disconnect {msg}");
                return;
            }

            await this.Run(session, msg);
        }

        public Type GetMessageType()
        {
            return typeof (Message);
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
}