using Common.Base;

namespace Model
{
    public enum GameObjectType
    {
        Player = 0,
    }

    public class GameObject: Entity
    {
        public GameObjectType Type { get; set; }
    }
}