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
        void Run(object o);
    }
	
    public interface IAwakeSystem<A>: ISystemType
    {
        void Run(object o, A a);
    }
	
    public interface IAwakeSystem<A, B>: ISystemType
    {
        void Run(object o, A a, B b);
    }
	
    public interface IAwakeSystem<A, B, C>: ISystemType
    {
        void Run(object o, A a, B b, C c);
    }
	
    public interface IAwakeSystem<A, B, C, D>: ISystemType
    {
        void Run(object o, A a, B b, C c, D d);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T> : IAwakeSystem where T: IAwake
    {
        public Type Type()
        {
            return typeof(T);
        }
		
        public Type SystemType()
        {
            return typeof(IAwakeSystem);
        }

        public void Run(object o)
        {
            this.Awake((T)o);
        }

        public abstract void Awake(T self);
    }
    
    [ObjectSystem]
    public abstract class AwakeSystem<T, A> : IAwakeSystem<A> where T: IAwake<A>
    {
        public Type Type()
        {
            return typeof(T);
        }
		
        public Type SystemType()
        {
            return typeof(IAwakeSystem<A>);
        }

        public void Run(object o, A a)
        {
            this.Awake((T)o, a);
        }

        public abstract void Awake(T self, A a);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T, A, B> : IAwakeSystem<A, B> where T: IAwake<A, B>
    {
        public Type Type()
        {
            return typeof(T);
        }
		
        public Type SystemType()
        {
            return typeof(IAwakeSystem<A, B>);
        }

        public void Run(object o, A a, B b)
        {
            this.Awake((T)o, a, b);
        }

        public abstract void Awake(T self, A a, B b);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T, A, B, C> : IAwakeSystem<A, B, C> where T: IAwake<A, B, C>
    {
        public Type Type()
        {
            return typeof(T);
        }
		
        public Type SystemType()
        {
            return typeof(IAwakeSystem<A, B, C>);
        }

        public void Run(object o, A a, B b, C c)
        {
            this.Awake((T)o, a, b, c);
        }

        public abstract void Awake(T self, A a, B b, C c);
    }
    
    [ObjectSystem]
    public abstract class AwakeSystem<T, A, B, C, D> : IAwakeSystem<A, B, C, D> where T: IAwake<A, B, C, D>
    {
        public Type Type()
        {
            return typeof(T);
        }
		
        public Type SystemType()
        {
            return typeof(IAwakeSystem<A, B, C, D>);
        }

        public void Run(object o, A a, B b, C c, D d)
        {
            this.Awake((T)o, a, b, c, d);
        }

        public abstract void Awake(T self, A a, B b, C c, D d);
    }
}