namespace Model
{
	public static class Game
	{
		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				if (scene != null)
				{
					return scene;
				}
				scene = new Scene();
				scene.AddComponent<EventComponent>();
				scene.AddComponent<TimerComponent>();
				return scene;
			}
		}
		
		public static void Close()
		{
			scene.Dispose();
			scene = null;
		}
	}
}