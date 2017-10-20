using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
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
