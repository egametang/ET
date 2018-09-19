namespace ETModel
{
	// 客户端发送actor消息
	public interface IClientActorMessage : IActorRequest
	{
	}

	// 客户端发送actor rpc消息
	public interface IClientActorRequest : IActorRequest
	{
	}
	
	public interface IClientActorResponse : IActorResponse
	{
	}
}