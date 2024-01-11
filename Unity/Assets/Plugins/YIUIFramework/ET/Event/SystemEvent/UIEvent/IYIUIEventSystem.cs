using System;

namespace ET.Client
{
    public interface IYIUIEvent<in A> where A : struct
    {
    }

    //A = 消息类型 = 如:YIUIEventPanelOpenBefore
    public interface IYIUIEventSystem<in A>: ISystemType where A : struct
    {
        ETTask Run(Entity o, A message);
    }

    [EntitySystem]
    public abstract class YIUIEventSystem<T, A>: IYIUIEventSystem<A> where T : Entity, IYIUIEvent<A> where A : struct
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIEventSystem<A>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.UIEvent;
        }

        public async ETTask Run(Entity o, A message)
        {
            await YIUIEvent((T)o, message);
        }

        protected abstract ETTask YIUIEvent(T self, A message);
    }
}