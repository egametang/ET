using System;

namespace ET.Client
{
    public interface IYIUIDisable
    {
    }

    public interface IYIUIDisableSystem: ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUIDisableSystem<T>: IYIUIDisableSystem where T : Entity, IYIUIDisable
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIDisableSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IYIUIDisableSystem.Run(Entity o)
        {
            this.YIUIDisable((T)o);
        }

        protected abstract void YIUIDisable(T self);
    }
}