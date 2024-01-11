using System;

namespace ET.Client
{
    public interface IYIUIInitialize
    {
    }

    public interface IYIUIInitializeSystem: ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUIInitializeSystem<T>: IYIUIInitializeSystem where T : Entity, IYIUIInitialize
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIInitializeSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IYIUIInitializeSystem.Run(Entity o)
        {
            this.YIUIInitialize((T)o);
        }

        protected abstract void YIUIInitialize(T self);
    }
}