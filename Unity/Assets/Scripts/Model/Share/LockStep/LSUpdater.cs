using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public class LSUpdater
    {
        [BsonIgnore]
        public LSWorld Parent { get; set; }
        
        private SortedSet<long> updateIds = new();

        private Dictionary<long, LSEntityRef<LSEntity>> lsEntities = new();

        private Queue<long> addUpdateIds = new();

        private Queue<long> removeUpdateIds = new();

        public void Update()
        {
            while (this.addUpdateIds.Count > 0)
            {
                this.updateIds.Add(this.addUpdateIds.Dequeue());
            }

            foreach (long id in this.updateIds)
            {
                LSEntity entity = lsEntities[id];
                if (entity == null)
                {
                    this.removeUpdateIds.Enqueue(id);
                    continue;
                }
                
                LSSington.Instance.LSUpdate(entity);
            }

            while (this.removeUpdateIds.Count > 0)
            {
                long id = this.removeUpdateIds.Dequeue();
                this.updateIds.Remove(id);
                this.lsEntities.Remove(id);
            }
        }
        
        public void Add(LSEntity entity)
        {
            this.addUpdateIds.Enqueue(entity.Id);
            this.lsEntities.Add(entity.Id, entity);
        }
    }
}