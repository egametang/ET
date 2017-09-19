namespace Model
{
	public class MessageInfo
	{
		public ushort Opcode { get; }
		public AMessage Message { get; set; }

		public MessageInfo(ushort opcode, AMessage message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
