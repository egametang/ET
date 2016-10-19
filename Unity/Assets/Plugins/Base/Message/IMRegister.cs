using System;

namespace Base
{
	public interface IMessageDispather
	{
		ushort GetOpcode(Type type);
		void RegisterHandler<Message>(ushort opcode, Action<Entity, Message> action) where Message : AMessage;

		void RegisterRpcHandler<Request, Response>(ushort opcode, Action<Entity, Request, Action<Response>> action) 
			where Request : ARequest
			where Response : AResponse;
	}

	public interface IMRegister
	{
		void Register(IMessageDispather messageDispather);
	}
}
