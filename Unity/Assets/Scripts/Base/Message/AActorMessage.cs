using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace Model
{
	[ProtoContract]
	[ProtoInclude(20000, typeof(AFrameMessage))]
	public abstract class AActorMessage : AMessage
	{
	}

	public abstract class AActorRequest : ARequest
	{
	}

	public abstract class AActorResponse : AResponse
	{
	}

	[ProtoContract]
	[ProtoInclude(30000, typeof(Frame_ClickMap))]
	[BsonKnownTypes(typeof(Frame_ClickMap))]
	public abstract class AFrameMessage : AActorMessage
	{
		[ProtoMember(1)]
		public long Id;
	}
}