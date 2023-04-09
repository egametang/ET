using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public class LSUpdater
    {
        [BsonIgnore]
        public LSScene Parent { get; set; }
        
        public int Frame { get; private set; }
        
        private SortedSet<long> updateIds = new();

        private Queue<long> addUpdateIds = new();

        private Queue<long> removeUpdateIds = new();

        public void Update()
        {
            ++this.Frame;

            // 让id保持从小到大update，后加进来的在一帧后面Update
            while (this.addUpdateIds.Count > 0)
            {
                this.updateIds.Add(this.addUpdateIds.Dequeue());
            }

            TypeSystems typeSystems = LSSington.Instance.TypeSystems;
            foreach (long id in this.updateIds)
            {
                LSEntity entity = this.Parent.Get(id);
                if (entity == null)
                {
                    this.removeUpdateIds.Enqueue(id);
                    continue;
                }
                
                List<object> iLSUpdateSystems = typeSystems.GetSystems(entity.GetType(), typeof (ILSUpdateSystem));
                if (iLSUpdateSystems == null)
                {
                    continue;
                }

                foreach (ILSUpdateSystem iLSUpdateSystem in iLSUpdateSystems)
                {
                    try
                    {
                        iLSUpdateSystem.Run(entity);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            while (this.removeUpdateIds.Count > 0)
            {
                this.updateIds.Remove(this.removeUpdateIds.Dequeue());
            }
        }
        
        public void Add(LSEntity entity)
        {
            this.addUpdateIds.Enqueue(entity.Id);
        }
    }
}