namespace Model
{
	public class MessageInfo
	{
		public ushort Opcode { get; }
		public object Message { get; set; }

		public MessageInfo(ushort opcode, object message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
