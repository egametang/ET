﻿using System;

namespace ET
{
	public interface IAddComponent
	{
	}
	
	public interface IAddComponentSystem: ISystemType
	{
		void Run(Entity o, Entity component);
	}

	[EntitySystem]
	public abstract class AddComponentSystem<T> : IAddComponentSystem where T: Entity, IAddComponent
	{
		void IAddComponentSystem.Run(Entity o, Entity component)
		{
			this.AddComponent((T)o, component);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IAddComponentSystem);
		}

		int ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.None;
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		protected abstract void AddComponent(T self, Entity component);
	}
}
