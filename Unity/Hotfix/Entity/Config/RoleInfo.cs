namespace ETModel
{
	[Config(AppType.Client)]
	public partial class RoleInfoCategory : ACategory<RoleInfo>
	{
	}

	public class RoleInfo: IConfig
	{
		public long Id { get; set; }
		public string Name;
		public string Desc;
		public int Height;
		public int Weight;
	}
}
