using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	[BsonKnownTypes(typeof(RechargeRecord))]
	[BsonKnownTypes(typeof(Recharge))]
	[BsonKnownTypes(typeof(Location))]
	public class EntityDB: Entity
	{
		protected EntityDB()
		{
		}

		protected EntityDB(long id): base(id)
		{
		}
	}
}
