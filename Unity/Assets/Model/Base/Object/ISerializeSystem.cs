using System;

namespace ETModel
{
	public interface IDeserializeSystem
	{
		Type Type();
		void Run(object o);
	}

	/// <summary>
	/// 反序列化后执行的System
	/// 使用<see cref="MessageHandlerAttribute"/>来指定在哪个服务器上运行,如未指定不会运行
	/// </summary>
	/// <typeparam name="T"></typeparam>
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
