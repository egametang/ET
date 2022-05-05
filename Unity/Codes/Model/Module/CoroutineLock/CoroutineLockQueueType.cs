using System.Collections.Generic;

namespace ET
{
    [FriendClass(typeof(CoroutineLockQueueType))]
    public static class CoroutineLockQueueTypeSystem
    {
        [ObjectSystem]
        public class CoroutineLockQueueTypeAwakeSystem: AwakeSystem<CoroutineLockQueueType>
        {
            public override void Awake(CoroutineLockQueueType self)
            {
                if (self.dictionary == null)
                {
                    self.dictionary = new Dictionary<long, CoroutineLockQueue>();
                }

                self.dictionary.Clear();
            }
        }

        [ObjectSystem]
        public class CoroutineLockQueueTypeDestroySystem: DestroySystem<CoroutineLockQueueType>
        {
            public override void Destroy(CoroutineLockQueueType self)
            {
                self.dictionary.Clear();
            }
        }
        
        public static bool TryGetValue(this CoroutineLockQueueType self, long key, out CoroutineLockQueue value)
        {
            return self.dictionary.TryGetValue(key, out value);
        }

        public static void Remove(this CoroutineLockQueueType self, long key)
        {
            if (self.dictionary.TryGetValue(key, out CoroutineLockQueue value))
            {
                value.Dispose();
            }
            self.dictionary.Remove(key);
        }
        
        public static void Add(this CoroutineLockQueueType self, long key, CoroutineLockQueue value)
        {
            self.dictionary.Add(key, value);
        }
    }
    
    public class CoroutineLockQueueType: Entity, IAwake, IDestroy
    {
        public Dictionary<long, CoroutineLockQueue> dictionary;
    }
}