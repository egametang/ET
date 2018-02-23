namespace Model
{
	public struct MessageInfo
	{
		public ushort Opcode { get; set; }
		public object Message { get; set; }

		public MessageInfo(ushort opcode, object message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
