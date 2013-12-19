
namespace Component
{
	public interface ILogic
	{
		void Handle(short opcode, byte[] content);
	}
}
