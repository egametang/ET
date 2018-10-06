namespace ETHotfix
{
	// 客户端发送actor消息
	public interface IClientActorMessage : IActorRequest
	{
	}

	// 客户端发送actor rpc消息
	public interface IActorLocationRequest : IActorRequest
	{
	}
	
	public interface IActorLocationResponse : IActorResponse
	{
	}
}