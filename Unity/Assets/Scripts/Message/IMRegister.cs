using System;
using Model;

namespace Model
{
	public interface IMessageDispather
	{
		ushort GetOpcode(Type type);

		void RegisterHandler<Message>(ushort opcode, Action<Session, Message> action) where Message : AMessage;

		void RegisterRpcHandler<Request, Response>(ushort opcode, Action<Session, Request, Action<Response>> action) 
			where Request : ARequest
			where Response : AResponse;
	}

	public interface IMRegister
	{
		void Register(IMessageDispather messageDispather);
	}
}
