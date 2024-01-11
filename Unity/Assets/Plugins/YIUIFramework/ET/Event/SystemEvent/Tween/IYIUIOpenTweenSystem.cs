using System;

namespace ET.Client
{
    public interface IYIUIOpenTween
    {
    }

    public interface IYIUIOpenTweenSystem: ISystemType
    {
        ETTask Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUIOpenTweenSystem<T>: IYIUIOpenTweenSystem where T : Entity, IYIUIOpenTween
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenTweenSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask IYIUIOpenTweenSystem.Run(Entity o)
        {
            await this.YIUIOpenTween((T)o);
        }

        protected abstract ETTask YIUIOpenTween(T self);
    }
}