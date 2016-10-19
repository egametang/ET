namespace Base
{
	public sealed class Game
	{
		private static Entity game;

		public static Entity Scene
		{
			get
			{
				if (game == null)
				{
					game = new Entity("Scene");
				}
				return game;
			}
		}

		public static void Close()
		{
			Entity scene = game;
			game = null;
			scene.Dispose();
		}
	}
}