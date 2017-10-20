using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class AActorMessage : AMessage
	{
	}

	public abstract class AActorRequest : ARequest
	{
	}

	public abstract class AActorResponse : AResponse
	{
	}

	[BsonKnownTypes(typeof(Frame_ClickMap))]
	public abstract class AFrameMessage : AActorMessage
	{
		public long Id;
	}
}