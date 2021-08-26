using System;

namespace ET
{
	public interface IUpdateSystem: ISystemType
	{
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
		
		public Type SystemType()
		{
			return typeof(IUpdateSystem);
		}

		public abstract void Update(T self);
	}
}
