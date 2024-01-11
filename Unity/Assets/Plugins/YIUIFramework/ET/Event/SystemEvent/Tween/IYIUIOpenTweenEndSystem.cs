using System;

namespace ET.Client
{
    public interface IYIUIOpenTweenEnd
    {
    }

    public interface IYIUIOpenTweenEndSystem: ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUIOpenTweenEndSystem<T>: IYIUIOpenTweenEndSystem where T : Entity, IYIUIOpenTweenEnd
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenTweenEndSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IYIUIOpenTweenEndSystem.Run(Entity o)
        {
            this.YIUIOpenTweenEnd((T)o);
        }

        protected abstract void YIUIOpenTweenEnd(T self);
    }
}