using System;

namespace ETHotfix
{
	public interface IDeserializeSystem
	{
		Type Type();
		void Run(object o);
	}

	public abstract class DeserializeSystem<T> : IDeserializeSystem
	{
		public void Run(object o)
		{
			this.Deserialize((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Deserialize(T self);
	}
}
