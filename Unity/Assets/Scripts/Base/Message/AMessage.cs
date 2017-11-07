using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

// 不要在这个文件加[ProtoInclude]跟[BsonKnowType]标签,加到InnerMessage.cs或者OuterMessage.cs里面去
namespace Model
{
	[ProtoContract]
	public abstract partial class AMessage
	{
		public override string ToString()
		{
			return MongoHelper.ToJson(this);
		}
	}

	[ProtoContract]
	public abstract partial class ARequest : AMessage
	{
		[ProtoMember(1000)]
		[BsonIgnoreIfDefault]
		public uint RpcId;
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	[ProtoContract]
	public abstract partial class AResponse : AMessage
	{
		[ProtoMember(1000)]
		public uint RpcId;

		[ProtoMember(1001)]
		public int Error = 0;

		[ProtoMember(1002)]
		public string Message = "";
	}
}