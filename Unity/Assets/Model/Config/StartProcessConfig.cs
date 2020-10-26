using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
	[Config]
	public partial class StartProcessConfigCategory : ACategory<StartProcessConfig>
	{
		public static StartProcessConfigCategory Instance;
		public StartProcessConfigCategory()
		{
			Instance = this;
		}
	}

	public partial class StartProcessConfig: IConfig
	{
		[BsonId]
		public long Id { get; set; }
		public int MachineId;
		public string InnerPort;
	}
}
