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
		public long Id { get; set; }
		public string InnerIP;
		public string InnerPort;
		public string OuterIP;
	}
}
