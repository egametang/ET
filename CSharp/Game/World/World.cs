using Component;

namespace World
{
    public class World
    {
        private static readonly World instance = new World();
        
        private readonly ConfigManager configManager = ConfigManager.Instance;

        private readonly GameObjectManager gameObjectManager = new GameObjectManager();

        public static World Instance
        {
            get
            {
                return instance;
            }
        }

        public ConfigManager ConfigManager
        {
            get
            {
                return this.configManager;
            }
        }

        public GameObjectManager GameObjectManager
        {
            get
            {
                return this.gameObjectManager;
            }
        }
    }
}