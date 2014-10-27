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
        }
    }
}