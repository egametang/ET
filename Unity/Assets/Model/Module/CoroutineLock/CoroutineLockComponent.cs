using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    using CoroutineLockQueue = SortedDictionary<int, ETTask<CoroutineLock>>;
    using CoroutineLockQueueType = Dictionary<long, SortedDictionary<int, ETTask<CoroutineLock>>>;
    
    public struct CoroutineLockTimer
    {
        public CoroutineLockType CoroutineLockType;
        public long Key;
        public int N;
        
        public CoroutineLockTimer(CoroutineLockType coroutineLockType, long key, int n)
        {
            this.CoroutineLockType = coroutineLockType;
            this.Key = key;
            this.N = n;
        }
    }
    
    [ObjectSystem]
    public class CoroutineLockComponentSystem: AwakeSystem<CoroutineLockComponent>
    {
        public override void Awake(CoroutineLockComponent self)
        {
            self.Awake();
        }
    }

    public class CoroutineLockComponentUpdateSystem: UpdateSystem<CoroutineLockComponent>
    {
        public override void Update(CoroutineLockComponent self)
        {
            self.Update();
        }
    }

    public class CoroutineLockComponent: Entity
    {
        public static CoroutineLockComponent Instance
        {
            get;
            private set;
        }

        private int n;
        
        private readonly Queue<CoroutineLockQueue> coroutineLockQueuePool = new Queue<CoroutineLockQueue>();

        private CoroutineLockQueue FetchCoroutineLockQueue()
        {
            if (this.coroutineLockQueuePool.Count == 0)
            {
                return new CoroutineLockQueue();
            }

            return this.coroutineLockQueuePool.Dequeue();
        }
        
        private void RecycleCoroutineLockQueue(CoroutineLockQueue coroutineLockQueue)
        {
            this.coroutineLockQueuePool.Enqueue(coroutineLockQueue);
        }

        private readonly List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>((int) CoroutineLockType.Max);

        private readonly Queue<(CoroutineLockType, long)> nextFrameRun = new Queue<(CoroutineLockType, long)>();

        private readonly MultiMap<long, CoroutineLockTimer> timers = new MultiMap<long, CoroutineLockTimer>();
        
        private readonly Queue<long> timeOutIds = new Queue<long>();

        private readonly Queue<CoroutineLockTimer> timerOutTimer = new Queue<CoroutineLockTimer>();

        private long minTime;

        public void Awake()
        {
            Instance = this;
            for (int i = 0; i < this.list.Capacity; ++i)
            {
                this.list.Add(new CoroutineLockQueueType());
            }
        }

        public void Update()
        {
            int count = this.nextFrameRun.Count;
            // 注意这里不能将this.nextFrameRun.Count 放到for循环中，因为循环过程中会有对象继续加入队列
            for (int i = 0; i < count; ++i)
            {
                (CoroutineLockType coroutineLockType, long key) = this.nextFrameRun.Dequeue();
                this.Notify(coroutineLockType, key, 1);
            }

            TimeoutCheck();
        }

        // 这里没有用TimerComponent，是为了避免每个计时器一个回调的gc
        private void TimeoutCheck()
        {
            // 超时的锁
            if (this.timers.Count == 0)
            {
                return;
            }

            long timeNow = TimeHelper.ClientFrameTime();

            if (timeNow < this.minTime)
            {
                return;
            }

            foreach (KeyValuePair<long, List<CoroutineLockTimer>> kv in this.timers)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    minTime = k;
                    break;
                }

                this.timeOutIds.Enqueue(k);
            }
            
            this.timerOutTimer.Clear();
            
            while (this.timeOutIds.Count > 0)
            {
                long time = this.timeOutIds.Dequeue();
                foreach (CoroutineLockTimer coroutineLockTimer in this.timers[time])
                {
                    this.timerOutTimer.Enqueue(coroutineLockTimer);
                }
                this.timers.Remove(time);
            }
            
            while (this.timerOutTimer.Count > 0)
            {
                CoroutineLockTimer coroutineLockTimer = this.timerOutTimer.Dequeue();

                CoroutineLockQueueType coroutineLockQueueType = this.list[(int) coroutineLockTimer.CoroutineLockType];
                if (!coroutineLockQueueType.TryGetValue(coroutineLockTimer.Key, out CoroutineLockQueue queue))
                {
                    continue;
                }
                if (!queue.TryGetValue(coroutineLockTimer.N, out ETTask<CoroutineLock> tcs))
                {
                    continue;
                }
                
                queue.Remove(coroutineLockTimer.N);
                
                if (queue.Count == 0)
                {
                    this.RecycleCoroutineLockQueue(queue);
                    coroutineLockQueueType.Remove(coroutineLockTimer.Key);
                }
                
                CoroutineLockType coroutineLockType = coroutineLockTimer.CoroutineLockType;
                long key = coroutineLockTimer.Key;
                
                tcs.SetException(new Exception($"coroutineLock timeout maybe have deadlock: {coroutineLockType} {key}"));
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

        public async ETTask<CoroutineLock> Wait(CoroutineLockType coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[(int) coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                coroutineLockQueueType.Add(key, FetchCoroutineLockQueue());

                return new CoroutineLock(this, coroutineLockType, key, 1);
            }

            ETTask<CoroutineLock> tcs = ETTask<CoroutineLock>.Create(true);
            
            int i = ++this.n;
            if (time > 0)
            {
                long tillTime = TimeHelper.ClientFrameTime() + time;
                CoroutineLockTimer coroutineLockTimer = new CoroutineLockTimer(coroutineLockType, key, i);
                this.timers.Add(tillTime, coroutineLockTimer);
                if (tillTime < this.minTime)
                {
                    this.minTime = tillTime;
                }
            }
            queue.Add(i, tcs);
            return await tcs;
        }

        public int GetCount(CoroutineLockType coroutineLockType, long key)
        {
            CoroutineLockQueueType coroutineLockQueueType = this.list[(int) coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                return 0;
            }

            return queue.Count;
        }

        public void Notify(CoroutineLockType coroutineLockType, long key, short index)
        {
            if (this.list.Count == 0) // 客户端关闭了
            {
                return;
            }
            CoroutineLockQueueType coroutineLockQueueType = this.list[(int) coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                return;
                // coroutineLockQueueType是空的也正常，因为有些协程锁可能超时删除了
                //throw new Exception($"first work notify not find queue");
            }

            if (queue.Count == 0)
            {
                this.RecycleCoroutineLockQueue(queue);
                coroutineLockQueueType.Remove(key);
                return;
            }
            
            // 注意因为协程锁Dispose会调用下一个协程，如果队列过多，堆栈可能溢出，所以这里限制了一次最多递归10层，
            // 超出则记录一下，下一帧再继续
            if (index > 10)
            {
                this.nextFrameRun.Enqueue((coroutineLockType, key));
                return;
            }

            var kv = queue.First();
            var tcs = kv.Value;
            queue.Remove(kv.Key);
            tcs.SetResult(new CoroutineLock(this, coroutineLockType, key, (short)(index + 1)));
        }
    }
}