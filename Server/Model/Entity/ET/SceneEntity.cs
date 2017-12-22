using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	// 需要记录自己所在Scene的继承这个类
	public class SceneEntity : Entity
	{
		[BsonIgnore]
		public Scene Scene { get; set; }

		protected SceneEntity()
		{
		}

		protected SceneEntity(long id) : base(id)
		{
		}
	}
}
