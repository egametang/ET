using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	[BsonKnownTypes(typeof(RechargeRecord))]
	[BsonKnownTypes(typeof(Recharge))]
	public class DBEntity: Entity
	{
		protected DBEntity()
		{
		}

		protected DBEntity(long id): base(id)
		{
		}
	}
}
