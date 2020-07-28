using System;

namespace ET
{
	public interface ILateUpdateSystem
	{
		Type Type();
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class LateUpdateSystem<T> : ILateUpdateSystem
	{
		public void Run(object o)
		{
			this.LateUpdate((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void LateUpdate(T self);
	}
}
