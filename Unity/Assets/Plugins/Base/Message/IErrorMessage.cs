namespace Base
{
	public abstract class AMessage
	{
	}

	public abstract class ARequest : AMessage
	{
		public uint RpcId;
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	public abstract class AResponse: AMessage
	{
		public uint RpcId;

		public int ErrorCode = 0;
		public string Message = "";
	}
}
