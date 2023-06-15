using System;

namespace ET
{
	public interface IDeserialize
	{
	}
	
	public interface IDeserializeSystem: ISystemType
	{
		void Run(Entity o);
	}

	/// <summary>
	/// 反序列化后执行的System
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[EntitySystem]
	public abstract class DeserializeSystem<T> : IDeserializeSystem where T: Entity, IDeserialize
	{
		void IDeserializeSystem.Run(Entity o)
		{
			this.Deserialize((T)o);
		}

		Type ISystemType.SystemType()
		{
			return typeof(IDeserializeSystem);
		}

		int ISystemType.GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.None;
		}

		Type ISystemType.Type()
		{
			return typeof(T);
		}

		protected abstract void Deserialize(T self);
	}
}
