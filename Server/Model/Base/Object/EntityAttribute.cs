using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(Location))]
	[BsonKnownTypes(typeof(Recharge))]
	[BsonKnownTypes(typeof(RechargeRecord))]
	public partial class Entity
	{
	}
}
