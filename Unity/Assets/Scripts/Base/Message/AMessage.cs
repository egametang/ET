using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace Model
{
	[ProtoContract]
	[BsonKnownTypes(typeof(AActorMessage))]
	[BsonKnownTypes(typeof(ARequest))]
	[BsonKnownTypes(typeof(AActorResponse))]
	public abstract partial class AMessage
	{
		public override string ToString()
		{
			return MongoHelper.ToJson(this);
		}
	}

	[ProtoContract]
	[BsonKnownTypes(typeof(AActorRequest))]
	public abstract partial class ARequest : AMessage
	{
		[ProtoMember(90)]
		[BsonIgnoreIfDefault]
		public uint RpcId;
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	[ProtoContract]
	[BsonKnownTypes(typeof(AActorResponse))]
	[BsonKnownTypes(typeof(ErrorResponse))]
	public abstract partial class AResponse : AMessage
	{
		[ProtoMember(90)]
		public uint RpcId;

		[ProtoMember(91)]
		public int Error = 0;

		[ProtoMember(92)]
		public string Message = "";
	}

	[ProtoContract]
	public class ErrorResponse: AResponse
	{
		
	}
}