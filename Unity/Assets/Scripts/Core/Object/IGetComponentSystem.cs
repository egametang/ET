using System;

namespace ET
{
	// GetComponentSystem有巨大作用，比如每次保存Unit的数据不需要所有组件都保存，只需要保存Unit变化过的组件
	// 是否变化可以通过判断该组件是否GetComponent，Get了就记录该组件
	// 这样可以只保存Unit变化过的组件
	// 再比如传送也可以做此类优化
	public interface IGetComponent
	{
	}
	
	public interface IGetComponentSystem: ISystemType
	{
		void Run(object o, Entity component);
	}

	[ObjectSystem]
	public abstract class GetComponentSystem<T> : IGetComponentSystem where T: IGetComponent
	{
		public void Run(object o, Entity component)
		{
			this.GetComponent((T)o, component);
		}
		
		public Type SystemType()
		{
			return typeof(IGetComponentSystem);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void GetComponent(T self, Entity component);
	}
}
