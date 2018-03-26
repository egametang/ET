using System;

namespace ETModel
{
	public interface IAwakeSystem
	{
		Type Type();
	}

	public interface IAwake
	{
		void Run(object o);
	}
	
	public interface IAwake<A>
	{
		void Run(object o, A a);
	}
	
	public interface IAwake<A, B>
	{
		void Run(object o, A a, B b);
	}
	
	public interface IAwake<A, B, C>
	{
		void Run(object o, A a, B b, C c);
	}

	public abstract class AwakeSystem<T> : IAwakeSystem, IAwake
	{
		public Type Type()
		{
			return typeof(T);
		}

		public void Run(object o)
		{
			this.Awake((T)o);
		}

		public abstract void Awake(T self);
	}

	public abstract class AwakeSystem<T, A> : IAwakeSystem, IAwake<A>
	{
		public Type Type()
		{
			return typeof(T);
		}

		public void Run(object o, A a)
		{
			this.Awake((T)o, a);
		}

		public abstract void Awake(T self, A a);
	}

	public abstract class AwakeSystem<T, A, B> : IAwakeSystem, IAwake<A, B>
	{
		public Type Type()
		{
			return typeof(T);
		}

		public void Run(object o, A a, B b)
		{
			this.Awake((T)o, a, b);
		}

		public abstract void Awake(T self, A a, B b);
	}

	public abstract class AwakeSystem<T, A, B, C> : IAwakeSystem, IAwake<A, B, C>
	{
		public Type Type()
		{
			return typeof(T);
		}

		public void Run(object o, A a, B b, C c)
		{
			this.Awake((T)o, a, b, c);
		}

		public abstract void Awake(T self, A a, B b, C c);
	}
}
