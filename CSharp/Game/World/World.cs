using Model;

namespace World
{
    public class World
    {
        private static readonly World instance = new World();

        private readonly ConfigManager configManager = new ConfigManager(typeof(World).Assembly);

        private readonly GameObjectManager gameObjectManager = new GameObjectManager();

        public static World Instance
        {
            get
            {
                return instance;
            }
        }

        private World()
        {
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