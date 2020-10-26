using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
	[Config]
	public partial class StartMachineConfigCategory : ACategory<StartMachineConfig>
	{
		public static StartMachineConfigCategory Instance;
		public StartMachineConfigCategory()
		{
			Instance = this;
		}
	}

	public partial class StartMachineConfig: IConfig
	{
		[BsonId]
		public long Id { get; set; }
		public string InnerIP;
		public string OuterIP;
	}
}
