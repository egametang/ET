using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	[BsonIgnoreExtraElements]
	public class BuffConfig: AConfig
	{
		public string Name { get; set; }
		public int Time { get; set; }

		public BuffConfig(): base(EntityType.Config)
		{
		}

		public BuffConfig(long id): base(id, EntityType.Config)
		{
		}
	}

	[Config(AppType.Client | AppType.Gate)]
	public class BuffCategory: ACategory<BuffConfig>
	{
	}
}