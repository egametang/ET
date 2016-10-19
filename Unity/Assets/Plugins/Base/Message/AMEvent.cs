using System;

namespace Base
{
	public abstract class AMEvent<Message>: IMRegister where Message: AMessage
	{
		protected abstract void Run(Entity scene, Message message);

		public void Register(IMessageDispather component)
		{
			ushort opcode = component.GetOpcode(typeof (Message));
			component.RegisterHandler<Message>(opcode, Run);
		}
	}

	public abstract class AMRpcEvent<Request, Response> : IMRegister
			where Request : ARequest
			where Response: AResponse
	{
		protected abstract void Run(Entity scene, Request message, Action<Response> reply);

		public void Register(IMessageDispather component)
		{
			ushort opcode = component.GetOpcode(typeof(Request));
			component.RegisterRpcHandler<Request, Response>(opcode, Run);
		}
	}
}