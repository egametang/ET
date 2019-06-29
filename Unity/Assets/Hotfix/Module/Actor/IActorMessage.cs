// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace ETHotfix
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
}