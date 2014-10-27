namespace Model
{
    public class World : GameObject<World>
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

            FactoryComponent<Unit> factoryComponent = this.AddComponent<FactoryComponent<Unit>>();
            factoryComponent.Load(new[] { typeof(World).Assembly });
        }
    }
}