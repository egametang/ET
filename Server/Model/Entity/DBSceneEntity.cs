using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	// 需要记录自己所在Scene的继承这个类
	public class DBSceneEntity : DBEntity
	{
		[BsonIgnore]
		public Scene Scene { get; set; }

		protected DBSceneEntity()
		{
		}

		protected DBSceneEntity(long id) : base(id)
		{
		}
	}
}
