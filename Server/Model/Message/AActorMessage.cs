using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(Actor_Test))]
	public abstract class AActorMessage : AMessage
	{
	}

	[BsonKnownTypes(typeof(ActorRpc_TestRequest))]
	public abstract class AActorRequest : ARequest
	{
	}

	public abstract class AActorResponse : AResponse
	{
	}
}