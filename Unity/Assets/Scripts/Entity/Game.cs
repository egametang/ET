using Model;

namespace Base
{
	public sealed class Game
	{
		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				return scene ?? (scene = new Scene());
			}
		}

		public static void Close()
		{
			Scene s = scene;
			scene = null;
			s.Dispose();
		}
	}
}