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
	public abstract class DestroySystem<T> : SystemObject, IDestroySystem where T: Entity, IDestroy
	{
		void IDestroySystem.Run(Entity o)
		{
			this.Destroy((T)o);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IDestroySystem);
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		protected abstract void Destroy(T self);
	}
}
