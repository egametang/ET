using System;

namespace ETModel
{
	public abstract class ALateUpdateSystem
	{
		public abstract Type Type();
		public abstract void Run(object o);
	}

	public abstract class LateUpdateSystem<T> : ALateUpdateSystem
	{
		public override void Run(object o)
		{
			this.LateUpdate((T)o);
		}

		public override Type Type()
		{
			return typeof(T);
		}

		public abstract void LateUpdate(T self);
	}
}
