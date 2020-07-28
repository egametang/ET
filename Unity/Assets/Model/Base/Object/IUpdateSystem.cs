using System;

namespace ET
{
	public interface IUpdateSystem
	{
		Type Type();
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class UpdateSystem<T> : IUpdateSystem
	{
		public void Run(object o)
		{
			this.Update((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Update(T self);
	}
}
