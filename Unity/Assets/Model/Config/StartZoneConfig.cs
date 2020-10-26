using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
	[Config]
	public partial class StartZoneConfigCategory : ACategory<StartZoneConfig>
	{
		public static StartZoneConfigCategory Instance;
		public StartZoneConfigCategory()
		{
			Instance = this;
		}
	}

	public partial class StartZoneConfig: IConfig
	{
		[BsonId]
		public long Id { get; set; }
		public string DBConnection;
		public string DBName;
	}
}
