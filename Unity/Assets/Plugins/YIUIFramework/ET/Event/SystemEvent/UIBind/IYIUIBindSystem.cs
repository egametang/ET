using System;

namespace ET.Client
{
    public interface IYIUIBind
    {
    }

    public interface IYIUIBindSystem: ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUIBindSystem<T>: IYIUIBindSystem where T : Entity, IYIUIBind
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIBindSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IYIUIBindSystem.Run(Entity o)
        {
            this.YIUIBind((T)o);
        }

        protected abstract void YIUIBind(T self);
    }
}