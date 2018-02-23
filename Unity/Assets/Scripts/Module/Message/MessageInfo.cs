namespace Model
{
	public struct MessageInfo
	{
		public uint RpcId { get; }
		public ushort Opcode { get; }
		public object Message { get; }

		public MessageInfo(uint rpcId, ushort opcode, object message)
		{
			this.RpcId = rpcId;
			this.Opcode = opcode;
			this.Message = message;
		}
	}
}
