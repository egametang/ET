namespace World
{
	public interface IHandle
	{
		void Handle(MessageEnv messageEnv, byte[] content);
	}
}
