using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace ETHotfix
{
	public interface IActorMessage: IRequest
	{
		long ActorId { get; set; }
	}

	[ProtoContract]
	public interface IActorRequest : IActorMessage
	{
	}

	[ProtoContract]
	public interface IActorResponse : IResponse
	{
	}

	[ProtoContract]
	public interface IFrameMessage : IMessage
	{
		long Id { get; set; }
	}
}