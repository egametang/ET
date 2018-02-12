using System;

namespace Model
{
	public abstract class ALoadSystem
	{
		public abstract Type Type();
		public abstract void Run(object o);
	}

	public abstract class LoadSystem<T> : ALoadSystem
	{
		public override void Run(object o)
		{
			this.Load((T)o);
		}

		public override Type Type()
		{
			return typeof(T);
		}

		public abstract void Load(T self);
	}
}
