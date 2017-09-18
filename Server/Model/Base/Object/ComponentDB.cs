using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	[BsonKnownTypes(typeof(UnitGateComponent))]
	public abstract class ComponentDB : Component
	{
	}
}