namespace Base
{
	public sealed class Game: Entity<Game>
	{
		private static Scene game;

		public static Scene Scene
		{
			get
			{
				if (game == null)
				{
					game = new Scene("Game", SceneType.Game);
				}
				return game;
			}
		}

		public static void Close()
		{
			Scene scene = game;
			game = null;
			scene.Dispose();
		}
	}
}