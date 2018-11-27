namespace ETModel
{
	[Config(AppType.ClientH |  AppType.ClientM | AppType.Gate | AppType.Map)]
	public partial class UnitConfigCategory : ACategory<UnitConfig>
	{
	}

	public class UnitConfig: IConfig
	{
		public long Id { get; set; }
		public string Name;
		public string Desc;
		public int Position;
		public int Height;
		public int Weight;
	}
}
