namespace Model
{
	public struct MessageInfo
	{
		public ushort Opcode { get; set; }
		public IMessage Message { get; set; }

		public MessageInfo(ushort opcode, IMessage message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
