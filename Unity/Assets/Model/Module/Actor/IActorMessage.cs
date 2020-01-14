namespace ETModel
{
	// 不需要返回消息
	public interface IActorMessage: IMessage
	{
		long ActorId { get; set; }
	}

	public interface IActorRequest : IActorMessage, IRequest
	{
	}

	public interface IActorResponse : IResponse
	{
	}
}