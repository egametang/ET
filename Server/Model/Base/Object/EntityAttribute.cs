using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonKnownTypes(typeof(Location))]
	[BsonKnownTypes(typeof(Recharge))]
	[BsonKnownTypes(typeof(RechargeRecord))]
	public partial class Entity
	{
	}
}
