using System;

namespace ET
{
    public interface IAwake
    {
    }

    public interface IAwake<A>
    {
    }
	
    public interface IAwake<A, B>
    {
    }
	
    public interface IAwake<A, B, C>
    {
    }
	
    public interface IAwake<A, B, C, D>
    {
    }
    
    public interface IAwakeSystem: ISystemType
    {
        void Run(Entity o);
    }
	
    public interface IAwakeSystem<A>: ISystemType
    {
        void Run(Entity o, A a);
    }
	
    public interface IAwakeSystem<A, B>: ISystemType
    {
        void Run(Entity o, A a, B b);
    }
	
    public interface IAwakeSystem<A, B, C>: ISystemType
    {
        void Run(Entity o, A a, B b, C c);
    }
	
    public interface IAwakeSystem<A, B, C, D>: ISystemType
    {
        void Run(Entity o, A a, B b, C c, D d);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T> : IAwakeSystem where T: Entity, IAwake
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IAwakeSystem.Run(Entity o)
        {
            this.Awake((T)o);
        }

        protected abstract void Awake(T self);
    }
    
    [EntitySystem]
    public abstract class AwakeSystem<T, A> : IAwakeSystem<A> where T: Entity, IAwake<A>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IAwakeSystem<A>.Run(Entity o, A a)
        {
            this.Awake((T)o, a);
        }

        protected abstract void Awake(T self, A a);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T, A, B> : IAwakeSystem<A, B> where T: Entity, IAwake<A, B>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A, B>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IAwakeSystem<A, B>.Run(Entity o, A a, B b)
        {
            this.Awake((T)o, a, b);
        }

        protected abstract void Awake(T self, A a, B b);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T, A, B, C> : IAwakeSystem<A, B, C> where T: Entity, IAwake<A, B, C>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A, B, C>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IAwakeSystem<A, B, C>.Run(Entity o, A a, B b, C c)
        {
            this.Awake((T)o, a, b, c);
        }

        protected abstract void Awake(T self, A a, B b, C c);
    }
    
    [EntitySystem]
    public abstract class AwakeSystem<T, A, B, C, D> : IAwakeSystem<A, B, C, D> where T: Entity, IAwake<A, B, C, D>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A, B, C, D>);
        }

        int ISystemType.GetInstanceQueueIndex()
        {
            return InstanceQueueIndex.None;
        }

        void IAwakeSystem<A, B, C, D>.Run(Entity o, A a, B b, C c, D d)
        {
            this.Awake((T)o, a, b, c, d);
        }

        protected abstract void Awake(T self, A a, B b, C c, D d);
    }
}