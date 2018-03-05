using System;

namespace ETModel
{
	public abstract class AUpdateSystem
	{
		public abstract Type Type();
		public abstract void Run(object o);
	}

	public abstract class UpdateSystem<T> : AUpdateSystem
	{
		public override void Run(object o)
		{
			this.Update((T)o);
		}

		public override Type Type()
		{
			return typeof(T);
		}

		public abstract void Update(T self);
	}
}
