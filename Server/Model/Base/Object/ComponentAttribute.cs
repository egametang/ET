using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	[BsonKnownTypes(typeof(UnitGateComponent))]
	[BsonKnownTypes(typeof(NumericComponent))]
	public partial class Component
	{
	}
}