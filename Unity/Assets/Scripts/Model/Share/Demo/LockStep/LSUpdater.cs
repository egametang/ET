using System;
using System.Collections.Generic;

namespace ET
{
    public class LSUpdater: Object
    {
        private List<long> updateIds = new();
        private List<long> newUpdateIds = new();

        private readonly Dictionary<long, EntityRef<LSEntity>> lsEntities = new();

        public void Update()
        {
            if (this.newUpdateIds.Count > 0)
            {
                foreach (long id in this.newUpdateIds)
                {
                    this.updateIds.Add(id);
                }
                this.updateIds.Sort();
                this.newUpdateIds.Clear();
            }

            foreach (long id in this.updateIds)
            {
                LSEntity entity = lsEntities[id];
                if (entity == null)
                {
                    this.lsEntities.Remove(id);
                    continue;
                }
                this.newUpdateIds.Add(id);
                LSEntitySystemSingleton.Instance.LSUpdate(entity);
            }
            this.updateIds.Clear();
            ObjectHelper.Swap(ref this.updateIds, ref this.newUpdateIds);
        }
        
        public void Add(LSEntity entity)
        {
            this.newUpdateIds.Add(entity.Id);
            this.lsEntities.Add(entity.Id, entity);
        }
    }
}