using System;

namespace ET
{
	public interface IDestroy
	{
		
	}
	
	public interface IDestroySystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class DestroySystem<T> : IDestroySystem where T: IDestroy
	{
		public void Run(object o)
		{
			this.Destroy((T)o);
		}
		
		public Type SystemType()
		{
			return typeof(IDestroySystem);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Destroy(T self);
	}
}
