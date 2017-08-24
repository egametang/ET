using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public interface IActorMessage
	{
		long Id { get; set; }
	}

	public abstract class AActorMessage: AMessage, IActorMessage
	{
		public long Id { get; set; }
	}

	public abstract class AActorRequest : ARequest, IActorMessage
	{
		[BsonIgnoreIfDefault]
		public long Id { get; set; }
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	public abstract class AActorResponse: AResponse
	{
	}
}