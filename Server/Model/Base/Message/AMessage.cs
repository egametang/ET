using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(ARequest))]
	[BsonKnownTypes(typeof(AResponse))]
	[BsonKnownTypes(typeof(AActorMessage))]
	public abstract class AMessage
	{
	}

	[BsonKnownTypes(typeof(AActorRequest))]
	public abstract class ARequest: AMessage
	{
		[BsonIgnoreIfDefault]
		public uint RpcId;
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	[BsonKnownTypes(typeof(AActorResponse))]
	public abstract class AResponse: AMessage
	{
		public uint RpcId;

		public int Error = 0;
		public string Message = "";
	}
}