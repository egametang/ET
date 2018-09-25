namespace ETModel
{
	// 不需要返回消息
	public interface IActorMessage: IMessage
	{
		long ActorId { get; set; }
	}

	public interface IActorRequest : IRequest
	{
		long ActorId { get; set; }
	}

	public interface IActorResponse : IResponse
	{
	}

	public interface IFrameMessage : IMessage
	{
		long Id { get; set; }
	}
}