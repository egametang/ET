
namespace Component
{
	[MessageAttribute(Opcode = 1)]
	public class CChat
	{
		public string Content { get; set; }
	}

	[MessageAttribute(Opcode = 2)]
	public class CLoginWorld
	{
	}
}
