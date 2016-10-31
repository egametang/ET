using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class BuffConfig : AConfig
	{
		public string Name { get; set; }
		public int Time { get; set; }
    }

	[Config(AppType.Client | AppType.Gate)]
	public class BuffCategory : ACategory<BuffConfig>
	{
	}
}