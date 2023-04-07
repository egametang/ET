using System;

namespace ET
{
    [FriendOf(typeof(FixedUpdateComponent))]
    public static class FixedUpdateComponentSystem
    {
        public static void Update(this FixedUpdateComponent self)
        {
            ++self.NowFrame;

            // 让id保持从小到大update，后加进来的在一帧后面Update
            while (self.addUpdateIds.Count > 0)
            {
                self.updateIds.Add(self.addUpdateIds.Dequeue());
            }

            foreach (long updateId in self.updateIds)
            {
                if (!self.updateEntities.TryGetValue(updateId, out FixedUpdateComponent.UpdateInfo updateInfo))
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
        
        public static long Add(this FixedUpdateComponent self, int type, Entity entity)
        {
            long updateId = ++self.idGenerator;
            FixedUpdateComponent.UpdateInfo updateInfo = new() { UpdateId = updateId, Entity = entity, Type = type };
            
            self.updateEntities.Add(updateId, updateInfo);
            
            self.addUpdateIds.Enqueue(updateId);
            return updateId;
        }
        
        public static void Remove(this FixedUpdateComponent self, long updateId)
        {
            self.updateEntities.Remove(updateId);
        }
    }
}