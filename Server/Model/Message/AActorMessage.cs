namespace Model
{
	public abstract class AActorMessage
	{
		public long Id { get; set; }
	}

	public abstract class AActorRequest : AMessage
	{
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	public abstract class AActorResponse
	{
		public int Error = 0;
		public string Message = "";
	}
}