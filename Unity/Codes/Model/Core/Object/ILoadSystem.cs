using System;

namespace ET
{
	public interface ILoad
	{
	}
	
	public interface ILoadSystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class LoadSystem<T> : ILoadSystem where T: ILoad
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
