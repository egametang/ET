using System;

namespace ET.Client
{
    public interface IYIUICloseTweenEnd
    {
    }

    public interface IYIUICloseTweenEndSystem: ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUICloseTweenEndSystem<T>: IYIUICloseTweenEndSystem where T : Entity, IYIUICloseTweenEnd
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUICloseTweenEndSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IYIUICloseTweenEndSystem.Run(Entity o)
        {
            this.YIUICloseTweenEnd((T)o);
        }

        protected abstract void YIUICloseTweenEnd(T self);
    }
}