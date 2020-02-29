namespace ET
{
    public static class Game
    {
        public static EventSystem EventSystem
        {
            get
            {
                return EventSystem.Instance;
            }
        }

        private static Scene scene;

        public static Scene Scene
        {
            get
            {
                return scene ?? (scene = EntitySceneFactory.CreateScene(1, SceneType.Process, "Process"));
            }
        }

        public static ObjectPool ObjectPool
        {
            get
            {
                return ObjectPool.Instance;
            }
        }
		
        public static void Close()
        {
            scene?.Dispose();
            scene = null;
            ObjectPool.Instance.Dispose();
            EventSystem.Instance.Dispose();
        }
    }
}