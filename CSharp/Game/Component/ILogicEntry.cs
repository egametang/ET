
namespace Component
{
	public interface ILogicEntry
	{
		void Enter(MessageEnv messageEnv, short opcode, byte[] content);
	}
}
