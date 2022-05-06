using System.Collections.Generic;

namespace ET
{
    public struct CoroutineLockInfo
    {
        public ETTask<CoroutineLock> Tcs;
        public int Time;
    }

    [FriendClass(typeof(CoroutineLockQueue))]
    public static class CoroutineLockQueueSystem
    {
        [ObjectSystem]
        public class CoroutineLockQueueAwakeSystem: AwakeSystem<CoroutineLockQueue>
        {
            public override void Awake(CoroutineLockQueue self)
            {
                self.queue.Clear();
            }
        }

        [ObjectSystem]
        public class CoroutineLockQueueDestroySystem: DestroySystem<CoroutineLockQueue>
        {
            public override void Destroy(CoroutineLockQueue self)
            {
                self.queue.Clear();
            }
        }
        
        public static void Add(this CoroutineLockQueue self, ETTask<CoroutineLock> tcs, int time)
        {
            self.queue.Enqueue(new CoroutineLockInfo(){Tcs = tcs, Time = time});
        }
        
        public static CoroutineLockInfo Dequeue(this CoroutineLockQueue self)
        {
            return self.queue.Dequeue();
        }
    }
    
    public class CoroutineLockQueue: Entity, IAwake, IDestroy
    {
        public Queue<CoroutineLockInfo> queue = new Queue<CoroutineLockInfo>();

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }
    }
}