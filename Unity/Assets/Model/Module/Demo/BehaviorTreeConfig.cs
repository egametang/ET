namespace ETModel
{
	[Config(AppType.ClientH |  AppType.ClientM | AppType.Gate | AppType.Map)]
	public partial class BehaviorTreeConfigCategory : ACategory<BehaviorTreeConfig>
	{
	}

	public class BehaviorTreeConfig: IConfig
	{
		public long Id { get; set; }
		public string Name;
		public string Description;
		public string ComponentName;
		public string ParameterList;
	}
}
