namespace Model
{
	public class StartConfig: AConfig
	{
		public int AppId { get; set; }

		public AppType AppType { get; set; }

		public string ServerIP { get; set; }

		public StartConfig(): base(EntityType.Config)
		{
		}
	}
}