using System;

namespace ET.Client
{
    public interface IYIUIOpenParam
    {
    }

    public interface IYIUIOpen
    {
    }

    public interface IYIUIOpen<A>: IYIUIOpenParam
    {
    }

    public interface IYIUIOpen<A, B>: IYIUIOpenParam
    {
    }

    public interface IYIUIOpen<A, B, C>: IYIUIOpenParam
    {
    }

    public interface IYIUIOpen<A, B, C, D>: IYIUIOpenParam
    {
    }

    public interface IYIUIOpen<A, B, C, D, E>: IYIUIOpenParam
    {
    }

    public interface IYIUIOpenSystem: ISystemType
    {
        ETTask<bool> Run(Entity o);
    }

    public interface IYIUIOpenSystem<A>: ISystemType
    {
        ETTask<bool> Run(Entity o, A a);
    }

    public interface IYIUIOpenSystem<A, B>: ISystemType
    {
        ETTask<bool> Run(Entity o, A a, B b);
    }

    public interface IYIUIOpenSystem<A, B, C>: ISystemType
    {
        ETTask<bool> Run(Entity o, A a, B b, C c);
    }

    public interface IYIUIOpenSystem<A, B, C, D>: ISystemType
    {
        ETTask<bool> Run(Entity o, A a, B b, C c, D d);
    }

    public interface IYIUIOpenSystem<A, B, C, D, E>: ISystemType
    {
        ETTask<bool> Run(Entity o, A a, B b, C c, D d, E e);
    }

    [EntitySystem]
    public abstract class YIUIOpenSystem<T>: IYIUIOpenSystem where T : Entity, IYIUIOpen
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask<bool> IYIUIOpenSystem.Run(Entity o)
        {
            return await this.YIUIOpen((T)o);
        }

        protected abstract ETTask<bool> YIUIOpen(T self);
    }

    [EntitySystem]
    public abstract class YIUIOpenSystem<T, A>: IYIUIOpenSystem<A> where T : Entity, IYIUIOpen<A>
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenSystem<A>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask<bool> IYIUIOpenSystem<A>.Run(Entity o, A a)
        {
            return await this.YIUIOpen((T)o, a);
        }

        protected abstract ETTask<bool> YIUIOpen(T self, A a);
    }

    [EntitySystem]
    public abstract class YIUIOpenSystem<T, A, B>: IYIUIOpenSystem<A, B> where T : Entity, IYIUIOpen<A, B>
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenSystem<A, B>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask<bool> IYIUIOpenSystem<A, B>.Run(Entity o, A a, B b)
        {
            return await this.YIUIOpen((T)o, a, b);
        }

        protected abstract ETTask<bool> YIUIOpen(T self, A a, B b);
    }

    [EntitySystem]
    public abstract class YIUIOpenSystem<T, A, B, C>: IYIUIOpenSystem<A, B, C> where T : Entity, IYIUIOpen<A, B, C>
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenSystem<A, B, C>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask<bool> IYIUIOpenSystem<A, B, C>.Run(Entity o, A a, B b, C c)
        {
            return await this.YIUIOpen((T)o, a, b, c);
        }

        protected abstract ETTask<bool> YIUIOpen(T self, A a, B b, C c);
    }

    [EntitySystem]
    public abstract class YIUIOpenSystem<T, A, B, C, D>: IYIUIOpenSystem<A, B, C, D> where T : Entity, IYIUIOpen<A, B, C, D>
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenSystem<A, B, C, D>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask<bool> IYIUIOpenSystem<A, B, C, D>.Run(Entity o, A a, B b, C c, D d)
        {
            return await this.YIUIOpen((T)o, a, b, c, d);
        }

        protected abstract ETTask<bool> YIUIOpen(T self, A a, B b, C c, D d);
    }

    [EntitySystem]
    public abstract class YIUIOpenSystem<T, A, B, C, D, E>: IYIUIOpenSystem<A, B, C, D, E> where T : Entity, IYIUIOpen<A, B, C, D, E>
    {
        Type ISystemType.Type()
        {
            return typeof (T);
        }

        Type ISystemType.SystemType()
        {
            return typeof (IYIUIOpenSystem<A, B, C, D, E>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        async ETTask<bool> IYIUIOpenSystem<A, B, C, D, E>.Run(Entity o, A a, B b, C c, D d, E e)
        {
            return await this.YIUIOpen((T)o, a, b, c, d, e);
        }

        protected abstract ETTask<bool> YIUIOpen(T self, A a, B b, C c, D d, E e);
    }
}