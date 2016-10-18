using System;

namespace Base
{
	public interface IMessageDispather
	{
		ushort GetOpcode(Type type);
		void RegisterHandler<T>(ushort opcode, Action<Entity, T> action);
		void RegisterRpcHandler<T>(ushort opcode, Action<Entity, T, uint> action);
	}

	public interface IMRegister
	{
		void Register(IMessageDispather messageDispather);
	}
}
