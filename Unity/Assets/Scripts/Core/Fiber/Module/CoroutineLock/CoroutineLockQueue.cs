using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(CoroutineLockQueue))]
    public static partial class CoroutineLockQueueSystem
    {
        [EntitySystem]
        private static void Awake(this CoroutineLockQueue self, int type)
        {
            self.type = type;
        }
        
        [EntitySystem]
        private static void Destroy(this CoroutineLockQueue self)
        {
            self.queue.Clear();
            self.type = 0;
            self.CurrentCoroutineLock = null;
        }
        
        public static async ETTask<CoroutineLock> Wait(this CoroutineLockQueue self, int time)
        {
            if (self.CurrentCoroutineLock == null)
            {
                self.CurrentCoroutineLock = self.AddChild<CoroutineLock, int, long, int>(self.type, self.Id, 1, true);
                return self.CurrentCoroutineLock;
            }

            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            self.queue.Enqueue(waitCoroutineLock);
            if (time > 0)
            {
                long tillTime = self.Fiber().TimeInfo.ClientFrameTime() + time;
                self.Root().GetComponent<TimerComponent>().NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            self.CurrentCoroutineLock = await waitCoroutineLock.Wait();
            return self.CurrentCoroutineLock;
        }

        public static void Notify(this CoroutineLockQueue self, int level)
        {
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (self.queue.Count > 0)
            {
                WaitCoroutineLock waitCoroutineLock = self.queue.Dequeue();

                if (waitCoroutineLock.IsDisposed())
                {
                    continue;
                }

                CoroutineLock coroutineLock = self.AddChild<CoroutineLock, int, long, int>(self.type, self.Id, level, true);

                waitCoroutineLock.SetResult(coroutineLock);
                break;
            }
        }
    }
    
    [ChildOf(typeof(CoroutineLockQueueType))]
    public class CoroutineLockQueue: Entity, IAwake<int>, IDestroy
    {
        public int type;

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