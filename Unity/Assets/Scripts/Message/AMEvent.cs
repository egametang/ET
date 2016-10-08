namespace Base
{
	public abstract class AMEvent<T>: IMRegister<MessageComponent>
	{
		public void Register(MessageComponent component)
		{
			component.Register<T>(Run);
		}

		public abstract void Run(Unit scene, T message);
	}
}