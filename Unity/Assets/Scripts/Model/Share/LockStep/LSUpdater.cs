using System;
using System.Collections.Generic;

namespace ET
{
    public class LSUpdater
    {
        private readonly SortedSet<long> updateIds = new();

        private readonly Dictionary<long, EntityRef<LSEntity>> lsEntities = new();

        private readonly Queue<long> addUpdateIds = new();

        private readonly Queue<long> removeUpdateIds = new();

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