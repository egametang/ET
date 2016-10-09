namespace Base
{
	public sealed class Game
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
			Entity scene = game;
			game = null;
			scene.Dispose();
		}
	}
}