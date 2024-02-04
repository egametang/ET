using System;

namespace ET
{
	public interface IDestroy
	{
	}
	
	public interface IDestroySystem: ISystemType
	{
		void Run(Entity o);
	}

	[EntitySystem]
	public abstract class DestroySystem<T> : IDestroySystem where T: Entity, IDestroy
	{
		void IDestroySystem.Run(Entity o)
		{
			this.Destroy((T)o);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IDestroySystem);
		}

		int ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.None;
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		protected abstract void Destroy(T self);
	}
}
