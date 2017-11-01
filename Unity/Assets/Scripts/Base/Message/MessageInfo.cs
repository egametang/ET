namespace Model
{
	public struct MessageInfo
	{
		public Opcode Opcode { get; set; }
		public AMessage Message { get; set; }

		public MessageInfo(Opcode opcode, AMessage message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
