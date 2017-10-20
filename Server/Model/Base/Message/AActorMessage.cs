using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

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
	[ProtoContract]
	[ProtoInclude(30000, typeof(Frame_ClickMap))]
	[BsonKnownTypes(typeof(Frame_ClickMap))]
	public abstract class AFrameMessage : AActorMessage
	{
		[ProtoMember(1)]
		public long Id;
	}
}