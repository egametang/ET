namespace Model
{
	[Config(AppType.Client)]
	public partial class UnitConfigCategory : ACategory<UnitConfig>
	{}

	public class UnitConfig: AConfig
	{
		public string Name;
		public string Desc;
		public int Position;
		public int Height;
		public int Weight;
	}
}
