using System;

namespace ET.Server
{
    [EnableClass]
    public abstract class AMActorHandler<E, Message>: IMActorHandler where E : Entity where Message : class, IActorMessage
    {
        protected abstract ETTask Run(E entity, Message message);

        public async ETTask Handle(Entity entity, int fromProcess, object actorMessage)
        {
            if (actorMessage is not Message msg)
            {
                Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Message).Name}");
                return;
            }

            if (entity is not E e)
            {
                Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof (E).Name} --{typeof (Message).Name}");
                return;
            }

            await this.Run(e, msg);
        }

        public Type GetRequestType()
        {
            if (typeof (IActorLocationMessage).IsAssignableFrom(typeof (Message)))
            {
                Log.Error($"message is IActorLocationMessage but handler is AMActorHandler: {typeof (Message)}");
            }

            return typeof (Message);
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
}