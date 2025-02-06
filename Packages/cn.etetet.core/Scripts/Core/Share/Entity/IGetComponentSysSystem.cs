using System;

namespace ET
{
	// GetComponentSystem有巨大作用，比如每次保存Unit的数据不需要所有组件都保存，只需要保存Unit变化过的组件
	// 是否变化可以通过判断该组件是否GetComponent，Get了就记录该组件
	// 这样可以只保存Unit变化过的组件
	// 再比如传送也可以做此类优化
	public interface IGetComponentSys
	{
	}
	
	public interface IGetComponentSysSystem: ISystemType
	{
		void Run(Entity o, Type type);
	}

	[EntitySystem]
	public abstract class GetComponentSysSystem<T> : SystemObject, IGetComponentSysSystem where T: Entity, IGetComponentSys
	{
		void IGetComponentSysSystem.Run(Entity o, Type type)
		{
			this.GetComponentSys((T)o, type);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IGetComponentSysSystem);
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		protected abstract void GetComponentSys(T self, Type type);
	}
}
