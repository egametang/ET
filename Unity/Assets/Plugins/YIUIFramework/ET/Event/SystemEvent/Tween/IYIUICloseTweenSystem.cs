using System;

namespace ET.Client
{
    public interface IYIUICloseTween
    {
    }

    public interface IYIUICloseTweenSystem: ISystemType
    {
        ETTask Run(Entity o);
    }

    [EntitySystem]
    public abstract class YIUICloseTweenSystem<T>: IYIUICloseTweenSystem where T : Entity, IYIUICloseTween
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUICloseTweenSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask IYIUICloseTweenSystem.Run(Entity o)
        {
            await this.YIUICloseTween((T)o);
        }

        protected abstract ETTask YIUICloseTween(T self);
    }
}