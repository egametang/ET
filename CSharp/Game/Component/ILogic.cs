
namespace Component
{
	public interface ILogic
	{
		void Handle(Opcode opcode, byte[] content);
	}
}
