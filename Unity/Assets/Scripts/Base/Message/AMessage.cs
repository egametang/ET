using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace Model
{
	[BsonKnownTypes(typeof(ARequest))]
	[BsonKnownTypes(typeof(AResponse))]
	[BsonKnownTypes(typeof(AActorMessage))]
	public abstract class AMessage
	{
		public override string ToString()
		{
			return MongoHelper.ToJson(this);
		}
	}

	[ProtoContract]
	[ProtoInclude(10000, typeof(C2R_Login))]
	[ProtoInclude(10001, typeof(C2G_LoginGate))]
	[ProtoInclude(10002, typeof(C2G_EnterMap))]
	[BsonKnownTypes(typeof(AActorRequest))]
	public abstract class ARequest : AMessage
	{
		[ProtoMember(1000)]
		[BsonIgnoreIfDefault]
		public uint RpcId;
	}

	/// <summary>
	/// 服务端回的RPC消息需要继承这个抽象类
	/// </summary>
	[ProtoContract]
	[ProtoInclude(10000, typeof(R2C_Login))]
	[ProtoInclude(10001, typeof(G2C_LoginGate))]
	[ProtoInclude(10002, typeof(G2C_EnterMap))]
	[BsonKnownTypes(typeof(AActorResponse))]
	public abstract class AResponse : AMessage
	{
		[ProtoMember(1000)]
		public uint RpcId;

		[ProtoMember(1001)]
		public int Error = 0;

		[ProtoMember(1002)]
		public string Message = "";
	}
}