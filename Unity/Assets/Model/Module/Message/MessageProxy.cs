using System;

namespace ET
{
    public class MessageProxy: IMHandler
    {
        private readonly Type type;
        private readonly Action<Session, object> action;

        public MessageProxy(Type type, Action<Session, object> action)
        {
            this.type = type;
            this.action = action;
        }

        public void Handle(Session session, object message)
        {
            this.action.Invoke(session, message);
        }

        public Type GetMessageType()
        {
            return this.type;
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
}