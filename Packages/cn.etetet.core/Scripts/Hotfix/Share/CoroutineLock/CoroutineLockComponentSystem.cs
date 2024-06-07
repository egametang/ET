namespace ET
{
    [EntitySystemOf(typeof(CoroutineLockComponent))]
    public static partial class CoroutineLockComponentSystem
    {
        [EntitySystem]
        public static void Awake(this CoroutineLockComponent self)
        {
        }
        
        [EntitySystem]
        public static void Update(this CoroutineLockComponent self)
        {
            // 循环过程中会有对象继续加入队列
            while (self.nextFrameRun.Count > 0)
            {
                (long coroutineLockType, long key, int count) = self.nextFrameRun.Dequeue();
                self.Notify(coroutineLockType, key, count);
            }
        }

        public static void RunNextCoroutine(this CoroutineLockComponent self, long coroutineLockType, long key, int level)
        {
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)
            {
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            }

            self.nextFrameRun.Enqueue((coroutineLockType, key, level));
        }

        public static async ETTask<CoroutineLock> Wait(this CoroutineLockComponent self, long coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.GetChild<CoroutineLockQueueType>(coroutineLockType) ?? self.AddChildWithId<CoroutineLockQueueType>(coroutineLockType);
            return await coroutineLockQueueType.Wait(key, time);
        }

        private static void Notify(this CoroutineLockComponent self, long coroutineLockType, long key, int level)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.GetChild<CoroutineLockQueueType>(coroutineLockType);
            if (coroutineLockQueueType == null)
            {
                return;
            }
            coroutineLockQueueType.Notify(key, level);
        }
    }
}