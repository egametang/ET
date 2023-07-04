using System;
using System.Collections.Generic;

namespace ET
{
    public class CoroutineLockQueue: IDisposable
    {
        private CoroutineLockComponent coroutineLockComponent;
        private int type;
        private long key;
        
        public static CoroutineLockQueue Create(CoroutineLockComponent coroutineLockComponent, int type, long key)
        {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.coroutineLockComponent = coroutineLockComponent; 
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
            if (this.currentCoroutineLock == null || this.currentCoroutineLock.IsDisposed)
            {
                this.currentCoroutineLock = CoroutineLock.Create(this.coroutineLockComponent, type, key, 1);
                return this.currentCoroutineLock;
            }

            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            this.queue.Enqueue(waitCoroutineLock);
            if (time > 0)
            {
                TimerComponent timerComponent = this.coroutineLockComponent.TimerComponent;
                long tillTime = timerComponent.TimeInfo.ClientFrameTime() + time;
                timerComponent.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
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

                CoroutineLock coroutineLock = CoroutineLock.Create(this.coroutineLockComponent, type, key, level);

                waitCoroutineLock.SetResult(coroutineLock);
                break;
            }
        }
        
        public bool IsDisposed
        {
            get
            {
                return this.coroutineLockComponent == null;
            }
        }

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            this.queue.Clear();
            this.key = 0;
            this.type = 0;
            this.currentCoroutineLock = null;
            this.coroutineLockComponent = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}