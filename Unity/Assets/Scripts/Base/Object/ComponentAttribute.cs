using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	public partial class Component
	{
	}
}