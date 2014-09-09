using Common.Config;
using Model;

namespace World
{
    public class World
    {
        private static readonly World instance = new World();

        private readonly ConfigManager configManager = new ConfigManager();

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
            configManager.Load(typeof(World).Assembly);
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