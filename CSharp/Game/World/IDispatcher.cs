
namespace World
{
	public interface IDispatcher
	{
		void Dispatch(MessageEnv messageEnv, short opcode, byte[] content);
	}
}
