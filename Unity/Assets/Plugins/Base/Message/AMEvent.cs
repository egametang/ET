using System;
using Base;

namespace Base
{
	public interface IMessageHandler
	{
		void Register<T>(ushort opcode, Action<Entity, T> action);
		void RegisterOpcode(Type type, ushort opcode);
	}

	public abstract class AMEvent<T>: IMRegister<IMessageHandler>
	{
		public abstract void Run(Entity scene, T message);

		public void Register(IMessageHandler component, ushort opcode)
		{
			component.RegisterOpcode(typeof(T), opcode);
			component.Register<T>(opcode, Run);
		}
	}
}