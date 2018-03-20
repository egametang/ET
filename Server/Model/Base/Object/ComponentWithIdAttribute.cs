using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonKnownTypes(typeof(Entity))]
	public partial class ComponentWithId
	{
	}
}