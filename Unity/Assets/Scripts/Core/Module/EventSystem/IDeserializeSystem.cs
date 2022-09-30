using System;

namespace ET
{
	public interface IDeserialize
	{
	}
	
	public interface IDeserializeSystem: ISystemType
	{
		void Run(object o);
	}

	/// <summary>
	/// 反序列化后执行的System
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[ObjectSystem]
	public abstract class DeserializeSystem<T> : IDeserializeSystem where T: IDeserialize
	{
		public void Run(object o)
		{
			this.Deserialize((T)o);
		}
		
		public Type SystemType()
		{
			return typeof(IDeserializeSystem);
		}

		public InstanceQueueIndex GetInstanceQueueIndex()
		{
			return InstanceQueueIndex.None;
		}

		public Type Type()
		{
			return typeof(T);
		}

		protected abstract void Deserialize(T self);
	}
}
