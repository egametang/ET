using System;

namespace Base
{
	public interface IMessageHandler
	{
		void RegisterHandler<T>(ushort opcode, Action<Entity, T, uint> action);
		ushort GetOpcode(Type type);
	}

	public interface IMRegister
	{
		void Register(IMessageHandler messageHandler);
	}
}
