namespace ETModel
{
	public static class Game
	{
		public static Scene Scene { get; set; }

		private static EventSystem eventSystem;

		public static EventSystem EventSystem
		{
			get
			{
				return eventSystem ?? (eventSystem = new EventSystem());
			}
		}

		private static ObjectPool objectPool;

		public static ObjectPool ObjectPool
		{
			get
			{
				return objectPool ?? (objectPool = new ObjectPool());
			}
		}

		public static Options Options;

		public static void Close()
		{
			Scene.Dispose();
			Scene = null;
					
			objectPool = null;
			
			eventSystem = null;
		}
	}
}