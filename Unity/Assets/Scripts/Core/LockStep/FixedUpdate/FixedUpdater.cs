using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [FriendOf(typeof(FixedUpdater))]
    public static class FixedUpdaterSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<FixedUpdater>
        {
            protected override void Awake(FixedUpdater self)
            {
                self.GetParent<LSScene>().FixedUpdater = self;
            }
        }
        
        public static void Update(this FixedUpdater self)
        {
            ++self.Frame;

            // 让id保持从小到大update，后加进来的在一帧后面Update
            while (self.addUpdateIds.Count > 0)
            {
                self.updateIds.Add(self.addUpdateIds.Dequeue());
            }

            foreach (long updateId in self.updateIds)
            {
                if (!self.updateEntities.TryGetValue(updateId, out FixedUpdater.UpdateInfo updateInfo))
                {
                    self.removeUpdateIds.Enqueue(updateId);
                    continue;
                }
                EventSystem.Instance.Invoke(updateInfo.Type, new FixUpdateInvokerArgs() {Entity = updateInfo.Entity});
            }

            while (self.removeUpdateIds.Count > 0)
            {
                self.updateIds.Remove(self.removeUpdateIds.Dequeue());
            }
        }
        
        public static long Add(this FixedUpdater self, int type, Entity entity)
        {
            long updateId = self.DomainScene().GetId();
            FixedUpdater.UpdateInfo updateInfo = new() { Entity = entity, Type = type };
            
            self.updateEntities.Add(updateId, updateInfo);
            
            self.addUpdateIds.Enqueue(updateId);
            return updateId;
        }
        
        public static void Remove(this FixedUpdater self, long updateId)
        {
            self.updateEntities.Remove(updateId);
        }
    }
    
    [ComponentOf(typeof(LSScene))]
    public class FixedUpdater: LSEntity, IAwake
    {
        public int Frame;
        
        public struct UpdateInfo
        {
            public int Type;
            public Entity Entity;
        }

        [BsonIgnore]
        public SortedSet<long> updateIds = new SortedSet<long>();
        [BsonIgnore]
        public Queue<long> addUpdateIds = new Queue<long>();
        [BsonIgnore]
        public Queue<long> removeUpdateIds = new Queue<long>();
        [BsonIgnore]
        public Dictionary<long, UpdateInfo> updateEntities = new Dictionary<long, UpdateInfo>();
    }
}