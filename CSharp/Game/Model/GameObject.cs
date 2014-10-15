using Common.Base;

namespace Model
{
    public enum GameObjectType
    {
        Player = 0,
    }

    public class GameObject: Entity
    {
        private GameObjectType type;

        public GameObjectType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }
}