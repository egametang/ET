using System;

namespace ET.Client
{
    public interface IYIUIEnable
    {
    }

    public interface IYIUIEnableSystem: ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUIEnableSystem<T>: IYIUIEnableSystem where T : Entity, IYIUIEnable
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIEnableSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IYIUIEnableSystem.Run(Entity o)
        {
            this.YIUIEnable((T)o);
        }

        protected abstract void YIUIEnable(T self);
    }
}