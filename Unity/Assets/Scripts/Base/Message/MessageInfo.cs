namespace Model
{
	public struct MessageInfo
	{
		public ushort Opcode { get; set; }
		public AMessage Message { get; set; }

		public MessageInfo(ushort opcode, AMessage message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
