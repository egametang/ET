using System;

namespace ET
{
	public struct LateUpdateEvent
	{
	}
	
	public interface ILateUpdate: IClassEvent<LateUpdateEvent>
	{
	}

	[EntitySystem]
	public abstract class LateUpdateSystem<T> : ClassEventSystem<T, LateUpdateEvent> where T: Entity, ILateUpdate
	{
		protected override void Handle(Entity e, LateUpdateEvent t)
		{
			this.LateUpdate((T)e);
		}

		protected abstract void LateUpdate(T self);
	}
}
