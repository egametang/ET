using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace Model
{
	[ProtoContract]
	public abstract partial class AActorMessage : AMessage
	{
	}

	[ProtoContract]
	public abstract partial class AActorRequest : ARequest
	{
	}

	[ProtoContract]
	public abstract partial class AActorResponse : AResponse
	{
	}

	[ProtoContract]
	public abstract partial class AFrameMessage : AActorMessage
	{
		[ProtoMember(1)]
		public long Id;
	}
}