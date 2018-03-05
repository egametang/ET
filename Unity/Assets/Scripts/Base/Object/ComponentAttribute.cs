using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	[BsonKnownTypes(typeof(Entity))]
	public partial class Component
	{
	}
}