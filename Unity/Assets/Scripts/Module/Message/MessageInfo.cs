namespace ETModel
{
	public struct MessageInfo
	{
		public ushort Opcode { get; }
		public object Message { get; }

		public MessageInfo(ushort opcode, object message)
		{
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
