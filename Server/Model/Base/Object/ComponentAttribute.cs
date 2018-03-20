using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	[BsonKnownTypes(typeof(UnitGateComponent))]
	[BsonKnownTypes(typeof(NumericComponent))]
	[BsonKnownTypes(typeof(ComponentWithId))]
	public partial class Component
	{
	}
}