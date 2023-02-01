using System;

namespace ET
{
	public interface ILoad
	{
	}
	
	public interface ILoadSystem: ISystemType
	{
		void Run(Entity o);
	}

	[ObjectSystem]
	public abstract class LoadSystem<T> : ILoadSystem where T: Entity, ILoad
	{
		void ILoadSystem.Run(Entity o)
		{
			this.Load((T)o);
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		Type ISystemType.SystemType()
		{
			return typeof(ILoadSystem);
		}

		InstanceQueueIndex ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.Load;
		}

		protected abstract void Load(T self);
	}
}
