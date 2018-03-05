namespace Model
{
	[Config(AppType.Client)]
	public partial class UnitConfigCategory : ACategory<UnitConfig>
	{
		public static void AvoidAOT(ConfigComponent configComponent, UnitConfigCategory category)
		{
			configComponent.Get<UnitConfig>(1);
			configComponent.GetCategory<UnitConfigCategory>();
			configComponent.GetAll<UnitConfig>();
			category.GetAll();
			UnitConfig config = category[1];
			config = category.TryGet(1);
			config = category.GetOne();
		}
	}

	public class UnitConfig: AConfig
	{
		public string Name;
		public string Desc;
		public int Position;
		public int Height;
		public int Weight;
	}
}
