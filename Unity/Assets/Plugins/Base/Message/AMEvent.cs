namespace Base
{
	public abstract class AMEvent<T>: IMRegister
	{
		public abstract void Run(Entity scene, T message, uint rpcId);

		public void Register(IMessageHandler component)
		{
			ushort opcode = component.GetOpcode(typeof (T));
			component.RegisterHandler<T>(opcode, Run);
		}
	}
}