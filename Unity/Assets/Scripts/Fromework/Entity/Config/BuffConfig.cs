namespace Model
{
	public class BuffConfig: AConfig
	{
		public string Name { get; set; }
		public int Duration { get; set; }

		public BuffConfig()
		{
		}

		public BuffConfig(long id): base(id)
		{
		}
	}

	[Config(AppType.Client)]
	public class BuffCategory: ACategory<BuffConfig>
	{
	}
}