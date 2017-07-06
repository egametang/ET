using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	[BsonKnownTypes(typeof(RechargeRecord))]
	[BsonKnownTypes(typeof(Recharge))]
	[BsonKnownTypes(typeof(DBSceneEntity))]
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
