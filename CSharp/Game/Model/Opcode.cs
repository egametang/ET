namespace Model
{
	public enum Opcode: short
	{
		CMsgLogin = 1,
		RpcResponse = 30000,
		RpcException = 30001,
	}
}