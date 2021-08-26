using System;

namespace ET
{
	public interface ILateUpdateSystem: ISystemType
	{
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
		
		public Type SystemType()
		{
			return typeof(ILateUpdateSystem);
		}

		public abstract void LateUpdate(T self);
	}
}
