using System;

namespace ETModel
{
	public interface ISerializeSystem
	{
		Type Type();
		void Run(object o);
	}

	/// <summary>
	/// 列化前执行的System
	/// 使用<see cref="MessageHandlerAttribute"/>来指定在哪个服务器上运行,如未指定不会运行
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SerializeSystem<T> : ISerializeSystem
	{
		public void Run(object o)
		{
			this.Serialize((T)o);
		}

		public Type Type()
		{
			return typeof(T);
		}

		public abstract void Serialize(T self);
	}
}
