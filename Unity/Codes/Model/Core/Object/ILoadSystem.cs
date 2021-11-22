using System;

namespace ET
{
	public interface ILoadSystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class LoadSystem<T> : ILoadSystem
	{
		public void Run(object o)
		{
			this.Load((T)o);
		}
		
		public Type Type()
		{
			return typeof(T);
		}
		
		public Type SystemType()
		{
			return typeof(ILoadSystem);
		}

		public abstract void Load(T self);
	}
}
