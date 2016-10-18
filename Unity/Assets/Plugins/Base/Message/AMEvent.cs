namespace Base
{
	public abstract class AMEvent<T>: IMRegister
	{
		protected abstract void Run(Entity scene, T message);

		public void Register(IMessageDispather component)
		{
			ushort opcode = component.GetOpcode(typeof (T));
			component.RegisterHandler<T>(opcode, Run);
		}
	}

	public abstract class AMRpcEvent<T> : IMRegister
	{
		protected abstract void Run(Entity scene, T message, uint rpcId);

		public void Register(IMessageDispather component)
		{
			ushort opcode = component.GetOpcode(typeof(T));
			component.RegisterRpcHandler<T>(opcode, Run);
		}
	}
}