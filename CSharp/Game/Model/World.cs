using Common.Config;
using Common.Factory;

namespace Model
{
    public class World : GameObject
    {
        private static readonly World instance = new World();

        public static World Instance
        {
            get
            {
                return instance;
            }
        }

        private World()
        {
            this.AddComponent<UnitComponent>();

            ConfigComponent configComponent = this.AddComponent<ConfigComponent>();
            configComponent.Load(new[] { typeof (World).Assembly });

            FactoryComponent factoryComponent = this.AddComponent<FactoryComponent>();
            factoryComponent.Load(new[] { typeof(World).Assembly });
        }
    }
}