using System;

namespace ETModel
{
	public interface IStartSystem
	{
		Type Type();
		void Run(object o);
	}

	public abstract class StartSystem<T> : IStartSystem
	{
		public void Run(object o)
		{
			this.Start((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Start(T self);
	}
}
