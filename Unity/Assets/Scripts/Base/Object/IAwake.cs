using System;

namespace Model
{
	public abstract class AAwakeSystem
	{
		public abstract Type Type();

		public virtual void Run(object o){ }
		public virtual void Run(object o, object a) { }
		public virtual void Run(object o, object a, object b) { }
		public virtual void Run(object o, object a, object b, object c) { }
	}

	public abstract class AwakeSystem<T> : AAwakeSystem
	{
		public override Type Type()
		{
			return typeof(T);
		}

		public override void Run(object o)
		{
			this.Awake((T)o);
		}

		public abstract void Awake(T self);
	}

	public abstract class AwakeSystem<T, A> : AAwakeSystem
	{
		public override Type Type()
		{
			return typeof(T);
		}

		public override void Run(object o, object a)
		{
			this.Awake((T)o, (A)a);
		}

		public abstract void Awake(T self, A a);
	}

	public abstract class AwakeSystem<T, A, B> : AAwakeSystem
	{
		public override Type Type()
		{
			return typeof(T);
		}

		public override void Run(object o, object a, object b)
		{
			this.Awake((T)o, (A)a, (B)b);
		}

		public abstract void Awake(T self, A a, B b);
	}

	public abstract class AwakeSystem<T, A, B, C> : AAwakeSystem
	{
		public override Type Type()
		{
			return typeof(T);
		}

		public override void Run(object o, object a, object b, object c)
		{
			this.Awake((T)o, (A)a, (B)b, (C)c);
		}

		public abstract void Awake(T self, A a, B b, C c);
	}
}
