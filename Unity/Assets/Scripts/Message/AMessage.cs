namespace Model
{
	public abstract class AMessage: Object
	{
		protected AMessage(): base(0)
		{
		}
	}

	public abstract class ARequest : AMessage
	{
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	public abstract class AResponse: AMessage
	{
		public int Error = 0;
		public string Message = "";
	}
}
