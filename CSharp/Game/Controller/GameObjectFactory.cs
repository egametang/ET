using Model;

namespace Controller
{
    public class GameObjectFactory
    {
        public static GameObject CreatePlayer()
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<BuffComponent>();
            return gameObject;
        }
    }
}
