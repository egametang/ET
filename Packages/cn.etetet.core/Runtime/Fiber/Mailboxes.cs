using System.Collections.Generic;

namespace ET
{
    public class Mailboxes
    {
        private readonly Dictionary<long, EntityRef<Entity>> mailboxes = new();
        
        public void Add(Entity mailBox)
        {
            this.mailboxes.Add(mailBox.Parent.InstanceId, mailBox);
        }
        
        public void Remove(long instanceId)
        {
            this.mailboxes.Remove(instanceId);
        }

        public EntityRef<Entity> Get(long instanceId)
        {
            this.mailboxes.TryGetValue(instanceId, out EntityRef<Entity> entity);
            return entity;
        }
    }
}