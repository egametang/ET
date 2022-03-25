using System;

namespace ET
{
	public interface IAddComponent
	{
	}
	
	public interface IAddComponentSystem: ISystemType
	{
		void Run(object o, Entity component);
	}

	[ObjectSystem]
	public abstract class AddComponentSystem<T> : IAddComponentSystem where T: IAddComponent
	{
		public void Run(object o, Entity component)
		{
			this.AddComponent((T)o, component);
		}
		
		public Type SystemType()
		{
			return typeof(IAddComponentSystem);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void AddComponent(T self, Entity component);
	}
}
