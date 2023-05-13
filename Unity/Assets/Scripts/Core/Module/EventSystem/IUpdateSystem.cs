using System;

namespace ET
{
	public interface IUpdate
	{
	}
	
	public interface IUpdateSystem: ISystemType
	{
		void Run(Entity o);
	}

	[EntitySystem]
	public abstract class UpdateSystem<T> : IUpdateSystem where T: Entity, IUpdate
	{
		void IUpdateSystem.Run(Entity o)
		{
			this.Update((T)o);
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IUpdateSystem);
		}

		int ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.Update;
		}

		protected abstract void Update(T self);
	}
}
