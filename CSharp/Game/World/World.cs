
namespace World
{
	public class World
	{
		private static readonly World instance = new World();

		private readonly Logic logic = Logic.Instance;

		private readonly Config config = Config.Instance;

		public static World Instance
		{
			get
			{
				return instance;
			}
		}

		public Logic Logic
		{
			get
			{
				return this.logic;
			}
		}

		public Config Config
		{
			get
			{
				return this.config;
			}
		}
	}
}
