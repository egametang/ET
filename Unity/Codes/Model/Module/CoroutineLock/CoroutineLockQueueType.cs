using System;
using System.Collections.Generic;

namespace ET
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
    
    public class CoroutineLockQueueType: Entity, IAwake, IDestroy
    {
        public Dictionary<long, CoroutineLockQueue> dictionary;

        public bool TryGetValue(long key, out CoroutineLockQueue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        public void Remove(long key)
        {
            if (this.dictionary.TryGetValue(key, out CoroutineLockQueue value))
            {
                value.Dispose();
            }
            this.dictionary.Remove(key);
        }
        
        public void Add(long key, CoroutineLockQueue value)
        {
            this.dictionary.Add(key, value);
        }
    }
}