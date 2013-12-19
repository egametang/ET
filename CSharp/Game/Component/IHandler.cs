
namespace Component
{
	public interface IHandler
	{
		void Handle(MessageEnv messageEnv, byte[] content);
	}
}
