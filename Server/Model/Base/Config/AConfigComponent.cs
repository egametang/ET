using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	[BsonKnownTypes(typeof (ClientConfig))]
	[BsonKnownTypes(typeof (InnerConfig))]
	[BsonKnownTypes(typeof (OuterConfig))]
	[BsonKnownTypes(typeof (HttpConfig))]
	[BsonKnownTypes(typeof (DBConfig))]
	public abstract class AConfigComponent: Component, ISerializeToEntity
	{
	}
}