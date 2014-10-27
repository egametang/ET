using Common.Base;

namespace Model
{
    public abstract class GameObject<K>: Entity<K> where K : Entity<K>
    {
    }
}