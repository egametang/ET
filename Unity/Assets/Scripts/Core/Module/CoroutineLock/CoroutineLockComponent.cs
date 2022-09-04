using System;
using System.Collections.Generic;

namespace ET
{
    public class CoroutineLockComponent: Singleton<CoroutineLockComponent>, ISingletonUpdate
    {
        private readonly List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>(CoroutineLockType.Max);
        private readonly Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>();

        public CoroutineLockComponent()
        {
            for (int i = 0; i < CoroutineLockType.Max; ++i)
            {
                CoroutineLockQueueType coroutineLockQueueType = new CoroutineLockQueueType(i);
                this.list.Add(coroutineLockQueueType);
            }
        }

        public override void Dispose()
        {
            this.list.Clear();
            this.nextFrameRun.Clear();
        }

        public void Update()
        {
            // 循环过程中会有对象继续加入队列
            while (this.nextFrameRun.Count > 0)
            {
                (int coroutineLockType, long key, int count) = this.nextFrameRun.Dequeue();
                this.Notify(coroutineLockType, key, count);
            }
        }

        public void RunNextCoroutine(int coroutineLockType, long key, int level)
        {
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)
            {
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            }

            this.nextFrameRun.Enqueue((coroutineLockType, key, level));
        }

        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];
            return await coroutineLockQueueType.Wait(key, time);
        }

        private void Notify(int coroutineLockType, long key, int level)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];
            coroutineLockQueueType.Notify(key, level);
        }
    }
}