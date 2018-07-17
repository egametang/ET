// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace ETModel
{
	public interface IActorMessage: IRequest
	{
		long ActorId { get; set; }
	}

	public interface IActorRequest : IActorMessage
	{
	}

	public interface IActorResponse : IResponse
	{
	}

	public interface IFrameMessage : IMessage
	{
		long Id { get; set; }
	}
}