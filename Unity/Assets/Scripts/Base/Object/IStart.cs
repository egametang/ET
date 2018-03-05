using System;

namespace ETModel
{
	public abstract class AStartSystem
	{
		public abstract Type Type();
		public abstract void Run(object o);
	}

	public abstract class StartSystem<T> : AStartSystem
	{
		public override void Run(object o)
		{
			this.Start((T)o);
		}

		public override Type Type()
		{
			return typeof(T);
		}

		public abstract void Start(T self);
	}
}
