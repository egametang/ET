namespace Model
{
	public static class Game
	{
		private static EntityEventManager entityEventManager;

		public static TPoller Poller { get; } = new TPoller();

		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				if (scene == null)
				{
					scene = new Scene();
					scene.AddComponent<EventComponent>();
					scene.AddComponent<TimerComponent>();
				}
				return scene;
			}
		}
		
		public static void CloseScene()
		{
			scene.Dispose();
			scene = null;
		}
		
		public static EntityEventManager EntityEventManager
		{
			get
			{
				if (entityEventManager == null)
				{
					entityEventManager = new EntityEventManager();
				}
				return entityEventManager;
			}
			set
			{
				entityEventManager = value;
			}
		}
	}
}