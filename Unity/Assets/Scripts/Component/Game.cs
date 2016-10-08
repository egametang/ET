namespace Base
{
	public sealed class Game
	{
		private static Unit game;

		public static Unit Scene
		{
			get
			{
				if (game == null)
				{
					game = new Unit();
					game.AddComponent<Scene>();
				}
				return game;
			}
		}

		public static void Close()
		{
			Unit scene = game;
			game = null;
			scene.Dispose();
		}
	}
}