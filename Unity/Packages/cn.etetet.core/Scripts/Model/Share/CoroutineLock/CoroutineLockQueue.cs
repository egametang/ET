using System;
using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(CoroutineLockQueueType))]
    public class CoroutineLockQueue: Entity, IAwake<long>, IDestroy
    {
        public long type;

        private EntityRef<CoroutineLock> currentCoroutineLock;

        public CoroutineLock CurrentCoroutineLock
        {
            get
            {
                return this.currentCoroutineLock;
            }
            set
            {
                this.currentCoroutineLock = value;
            }
        }
        
        public Queue<WaitCoroutineLock> queue = new();

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }
    }
}