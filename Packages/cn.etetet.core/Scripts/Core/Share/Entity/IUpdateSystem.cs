using System;

namespace ET
{
	public struct UpdateEvent
	{
	}
	
	public interface IUpdate: IClassEvent<UpdateEvent>
	{
	}

	[EntitySystem]
	public abstract class UpdateSystem<T> : ClassEventSystem<T, UpdateEvent> where T: Entity, IUpdate
	{
		protected override void Handle(Entity e, UpdateEvent t)
		{
			this.Update((T)e);
		}

		protected abstract void Update(T self);
	}
}
