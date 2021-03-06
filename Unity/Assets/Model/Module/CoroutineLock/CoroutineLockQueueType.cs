using System.Collections.Generic;

namespace ETModel
{
    public class CoroutineLockQueueType: Entity
    {
        private readonly Dictionary<long, CoroutineLockQueue> workQueues = new Dictionary<long, CoroutineLockQueue>();

        public void Add(long key, CoroutineLockQueue coroutineLockQueue)
        {
            this.workQueues.Add(key, coroutineLockQueue);
            coroutineLockQueue.Parent = this;
        }

        public void Remove(long key)
        {
            if (!this.workQueues.TryGetValue(key, out CoroutineLockQueue queue))
            {
                return;
            }

            this.workQueues.Remove(key);
            queue.Dispose();
        }

        public bool ContainsKey(long key)
        {
            return this.workQueues.ContainsKey(key);
        }

        public bool TryGetValue(long key, out CoroutineLockQueue coroutineLockQueue)
        {
            return this.workQueues.TryGetValue(key, out coroutineLockQueue);
        }
    }
}