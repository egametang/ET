using System;

namespace ET
{
	public interface ISerialize
	{
	}
	
	public interface ISerializeSystem: ISystemType
	{
		void Run(Entity o);
	}

	/// <summary>
	/// 序列化前执行的System
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[EntitySystem]
	public abstract class SerializeSystem<T> : ISerializeSystem where T: Entity, ISerialize
	{
		void ISerializeSystem.Run(Entity o)
		{
			this.Serialize((T)o);
		}

		Type ISystemType.SystemType()
		{
			return typeof(ISerializeSystem);
		}

		int ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.None;
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		protected abstract void Serialize(T self);
	}
}
