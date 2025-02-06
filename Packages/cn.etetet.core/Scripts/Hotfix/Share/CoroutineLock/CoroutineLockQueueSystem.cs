namespace ET
{
    [EntitySystemOf(typeof(CoroutineLockQueue))]
    public static partial class CoroutineLockQueueSystem
    {
        [EntitySystem]
        private static void Awake(this CoroutineLockQueue self, long type)
        {
            self.isStart = false;
            self.type = type;
        }
        
        [EntitySystem]
        private static void Destroy(this CoroutineLockQueue self)
        {
            self.queue.Clear();
            self.type = 0;
            self.isStart = false;
        }
        
        public static async ETTask<CoroutineLock> Wait(this CoroutineLockQueue self, int time)
        {
            CoroutineLock coroutineLock = null;
            if (!self.isStart)
            {
                self.isStart = true;
                coroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.type, self.Id, 1, true);
                return coroutineLock;
            }

            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            self.queue.Enqueue(waitCoroutineLock);
            if (time > 0)
            {
                long tillTime = TimeInfo.Instance.ClientFrameTime() + time;
                self.Root().GetComponent<TimerComponent>().NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            coroutineLock = await waitCoroutineLock.Wait();
            return coroutineLock;
        }

        // 返回值，是否找到了一个有效的协程锁
        public static bool Notify(this CoroutineLockQueue self, int level)
        {
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (self.queue.Count > 0)
            {
                WaitCoroutineLock waitCoroutineLock = self.queue.Dequeue();

                if (waitCoroutineLock.IsDisposed())
                {
                    continue;
                }

                CoroutineLock coroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.type, self.Id, level, true);

                waitCoroutineLock.SetResult(coroutineLock);
                return true;
            }
            return false;
        }
    }
}