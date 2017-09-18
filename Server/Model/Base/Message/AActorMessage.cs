using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(Actor_Test))]
	public abstract class AActorMessage : AMessage
	{
	}

	[BsonKnownTypes(typeof(Actor_TestRequest))]
	[BsonKnownTypes(typeof(Actor_TransferRequest))]
	public abstract class AActorRequest : ARequest
	{
	}

	public abstract class AActorResponse : AResponse
	{
	}
}