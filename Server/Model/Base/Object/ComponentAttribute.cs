using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	[BsonKnownTypes(typeof(UnitGateComponent))]
	[BsonKnownTypes(typeof(NumericComponent))]
	[BsonKnownTypes(typeof(Entity))]
	public partial class Component
	{
	}
}