namespace Model
{
	public enum Opcode: ushort
	{
		CMsgLogin = 1,
		RpcResponse = 30000,
		RpcException = 30001,
	}
}