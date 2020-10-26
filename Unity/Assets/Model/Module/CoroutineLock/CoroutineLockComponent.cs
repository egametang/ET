using System;
using System.Collections.Generic;

namespace ET
{
    
    public class CoroutineLockComponentSystem : AwakeSystem<CoroutineLockComponent>
    {
        public override void Awake(CoroutineLockComponent self)
        {
            self.Awake();
        }
    }
    
    public class CoroutineLockComponent: Entity
    {
        public static CoroutineLockComponent Instance { get; private set; }

        private readonly List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>((int)CoroutineLockType.Max);

        public void Awake()
        {
            Instance = this;
            for (int i = 0; i < this.list.Capacity; ++i)
            {
                CoroutineLockQueueType coroutineLockQueueType = EntityFactory.Create<CoroutineLockQueueType>(this.Domain);
                this.list.Add(coroutineLockQueueType);
                coroutineLockQueueType.Parent = this;
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.list.Clear();
        }

        public async ETTask<CoroutineLock> Wait(CoroutineLockType coroutineLockType, long key)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[(int) coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                queue = EntityFactory.Create<CoroutineLockQueue>(this.Domain);
                coroutineLockQueueType.Add(key, queue);
                
                return EntityFactory.CreateWithParent<CoroutineLock, CoroutineLockType, long>(this, coroutineLockType, key);
            }
            
            ETTaskCompletionSource<CoroutineLock> tcs = new ETTaskCompletionSource<CoroutineLock>();
            queue.Enqueue(tcs);
            return await tcs.Task;
        }
        
        public void Notify(CoroutineLockType coroutineLockType, long key)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[(int) coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                throw new Exception($"first work notify not find queue");
            }
            if (queue.Count == 0)
            {
                coroutineLockQueueType.Remove(key);
                queue.Dispose();
                return;
            }

            ETTaskCompletionSource<CoroutineLock> tcs = queue.Dequeue();
            tcs.SetResult(EntityFactory.CreateWithParent<CoroutineLock, CoroutineLockType, long>(this, coroutineLockType, key));
        }
    }
}