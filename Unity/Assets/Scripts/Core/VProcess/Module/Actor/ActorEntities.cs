using System.Collections.Generic;

namespace ET
{
    public class ActorEntities
    {
        private readonly Dictionary<ActorId, Entity> actors = new();
        
        public void Add(Entity entity)
        {
            this.actors.Add(entity.GetActorId(), entity);
        }
        
        public void Remove(Entity entity)
        {
            this.actors.Add(entity.GetActorId(), entity);
        }

        public Entity Get(ActorId actorId)
        {
            this.actors.TryGetValue(actorId, out Entity entity);
            return entity;
        }
    }
}