using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(Actor_Test))]
	[BsonKnownTypes(typeof(AFrameMessage))]
	[BsonKnownTypes(typeof(Actor_CreateUnits))]
	[BsonKnownTypes(typeof(FrameMessage))]
	public abstract class AActorMessage : AMessage
	{
	}

	[BsonKnownTypes(typeof(Actor_TestRequest))]
	[BsonKnownTypes(typeof(Actor_TransferRequest))]
	public abstract class AActorRequest : ARequest
	{
	}

	[BsonKnownTypes(typeof(Actor_TestResponse))]
	[BsonKnownTypes(typeof(Actor_TransferResponse))]
	public abstract class AActorResponse : AResponse
	{
	}

	/// <summary>
	/// 帧消息，继承这个类的消息会经过服务端转发
	/// </summary>
	[BsonKnownTypes(typeof(Frame_ClickMap))]
	public abstract class AFrameMessage : AActorMessage
	{
		public long Id;
	}
}