using System;

namespace ET
{
	public interface IGetComponent
	{
	}
	
	public interface IGetComponentSystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class GetComponentSystem<T> : IGetComponentSystem where T: IGetComponent
	{
		public void Run(object o)
		{
			this.GetComponent((T)o);
		}
		
		public Type SystemType()
		{
			return typeof(IGetComponentSystem);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void GetComponent(T self);
	}
}
