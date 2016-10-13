namespace Base
{
	public abstract class AMEvent<T>: IMRegister<MessageHandlerComponent>
	{
		public void Register(MessageHandlerComponent component)
		{
			component.Register<T>(Run);
		}

		public abstract void Run(Entity scene, T message);
	}
}