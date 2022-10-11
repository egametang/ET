using System;
using System.Collections.Generic;

namespace ET
{
    public class CoroutineLockQueue
    {
        private int type;
        private long key;
        
        public static CoroutineLockQueue Create(int type, long key)
        {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }

        private CoroutineLock currentCoroutineLock;
        
        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>();

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }

        public async ETTask<CoroutineLock> Wait(int time)
        {
            if (this.currentCoroutineLock == null)
            {
                this.currentCoroutineLock = CoroutineLock.Create(type, key, 1);
                return this.currentCoroutineLock;
            }

            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            this.queue.Enqueue(waitCoroutineLock);
            if (time > 0)
            {
                long tillTime = TimeHelper.ClientFrameTime() + time;
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            this.currentCoroutineLock = await waitCoroutineLock.Wait();
            return this.currentCoroutineLock;
        }

        public void Notify(int level)
        {
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (this.queue.Count > 0)
            {
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();

                if (waitCoroutineLock.IsDisposed())
                {
                    continue;
                }

                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level);

                waitCoroutineLock.SetResult(coroutineLock);
                break;
            }
        }

        public void Recycle()
        {
            this.queue.Clear();
            this.key = 0;
            this.type = 0;
            this.currentCoroutineLock = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}