using Base;

namespace Model
{
	public abstract class AMEvent<T>: IMRegister<MessageHandlerComponent>
	{
		public void Register(MessageHandlerComponent component, ushort opcode)
		{
			component.MessageOpcode[typeof(T)] = opcode;
			component.Register<T>(opcode, Run);
		}

		public abstract void Run(Entity scene, T message);
	}
}