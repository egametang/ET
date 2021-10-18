using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class CoroutineLockComponentAwakeSystem: AwakeSystem<CoroutineLockComponent>
    {
        public override void Awake(CoroutineLockComponent self)
        {
            CoroutineLockComponent.Instance = self;
            for (int i = 0; i < self.list.Capacity; ++i)
            {
                self.list.Add(self.AddChildWithId<CoroutineLockQueueType>(++self.idGenerator));
            }
        }
    }

    [ObjectSystem]
    public class CoroutineLockComponentDestroySystem: DestroySystem<CoroutineLockComponent>
    {
        public override void Destroy(CoroutineLockComponent self)
        {
            self.list.Clear();
            self.nextFrameRun.Clear();
            self.timers.Clear();
            self.timeOutIds.Clear();
            self.timerOutTimer.Clear();
            self.idGenerator = 0;
            self.minTime = 0;
        }
    }

    public class CoroutineLockComponentUpdateSystem: UpdateSystem<CoroutineLockComponent>
    {
        public override void Update(CoroutineLockComponent self)
        {
            // 检测超时的CoroutineLock
            TimeoutCheck(self);
            
            int count = self.nextFrameRun.Count;
            // 注意这里不能将this.nextFrameRun.Count 放到for循环中，因为循环过程中会有对象继续加入队列
            for (int i = 0; i < count; ++i)
            {
                (CoroutineLockType coroutineLockType, long key) = self.nextFrameRun.Dequeue();
                self.Notify(coroutineLockType, key, 0);
            }
        }
        
        public void TimeoutCheck(CoroutineLockComponent self)
        {
            // 超时的锁
            if (self.timers.Count == 0)
            {
                return;
            }

            long timeNow = TimeHelper.ClientFrameTime();

            if (timeNow < self.minTime)
            {
                return;
            }

            foreach (KeyValuePair<long, List<CoroutineLockTimer>> kv in self.timers)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    self.minTime = k;
                    break;
                }

                self.timeOutIds.Enqueue(k);
            }
            
            self.timerOutTimer.Clear();
            
            while (self.timeOutIds.Count > 0)
            {
                long time = self.timeOutIds.Dequeue();
                foreach (CoroutineLockTimer coroutineLockTimer in self.timers[time])
                {
                    self.timerOutTimer.Enqueue(coroutineLockTimer);
                }
                self.timers.Remove(time);
            }
            
            while (self.timerOutTimer.Count > 0)
            {
                CoroutineLockTimer coroutineLockTimer = self.timerOutTimer.Dequeue();
                if (coroutineLockTimer.CoroutineLockInstanceId != coroutineLockTimer.CoroutineLock.InstanceId)
                {
                    continue;
                }

                CoroutineLock coroutineLock = coroutineLockTimer.CoroutineLock;
                // 超时直接调用下一个锁
                self.NextFrameRun(coroutineLock.coroutineLockType, coroutineLock.key);
                coroutineLock.coroutineLockType = CoroutineLockType.None; // 上面调用了下一个, dispose不再调用
            }
        }
    }

    public static class CoroutineLockComponentSystem
    {
        public static void NextFrameRun(this CoroutineLockComponent self, CoroutineLockType coroutineLockType, long key)
        {
            self.nextFrameRun.Enqueue((coroutineLockType, key));
        }

        public static void AddTimer(this CoroutineLockComponent self, long tillTime, CoroutineLock coroutineLock)
        {
            self.timers.Add(tillTime, new CoroutineLockTimer(coroutineLock));
            if (tillTime < self.minTime)
            {
                self.minTime = tillTime;
            }
        }

        public static async ETTask<CoroutineLock> Wait(this CoroutineLockComponent self, CoroutineLockType coroutineLockType, long key, int time = 60000)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.list[(int) coroutineLockType];
   
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                coroutineLockQueueType.Add(key, self.AddChildWithId<CoroutineLockQueue>(++self.idGenerator, true));
                return self.CreateCoroutineLock(coroutineLockType, key, time, 1);
            }

            ETTask<CoroutineLock> tcs = ETTask<CoroutineLock>.Create(true);
            queue.Add(tcs, time);
            
            return await tcs;
        }

        public static CoroutineLock CreateCoroutineLock(this CoroutineLockComponent self, CoroutineLockType coroutineLockType, long key, int time, int count)
        {
            CoroutineLock coroutineLock = self.AddChildWithId<CoroutineLock, CoroutineLockType, long, int>(++self.idGenerator, coroutineLockType, key, count, true);
            if (time > 0)
            {
                self.AddTimer(TimeHelper.ClientFrameTime() + time, coroutineLock);
            }
            return coroutineLock;
        }

        public static void Notify(this CoroutineLockComponent self, CoroutineLockType coroutineLockType, long key, int count)
        {
            CoroutineLockQueueType coroutineLockQueueType = self.list[(int) coroutineLockType];
            if (!coroutineLockQueueType.TryGetValue(key, out CoroutineLockQueue queue))
            {
                return;
            }

            if (queue.Count == 0)
            {
                coroutineLockQueueType.Remove(key);
                return;
            }
            
#if NOT_UNITY
            const int frameCoroutineCount = 5;
#else
            const int frameCoroutineCount = 10;
#endif

            if (count > frameCoroutineCount)
            {
                self.NextFrameRun(coroutineLockType, key);
                return;
            }
            
            CoroutineLockInfo coroutineLockInfo = queue.Dequeue();
            coroutineLockInfo.Tcs.SetResult(self.CreateCoroutineLock(coroutineLockType, key, coroutineLockInfo.Time, count));
        }
    }

    public class CoroutineLockComponent: Entity
    {
        public static CoroutineLockComponent Instance;
        
        public List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>((int) CoroutineLockType.Max);
        public Queue<(CoroutineLockType, long)> nextFrameRun = new Queue<(CoroutineLockType, long)>();
        public MultiMap<long, CoroutineLockTimer> timers = new MultiMap<long, CoroutineLockTimer>();
        public Queue<long> timeOutIds = new Queue<long>();
        public Queue<CoroutineLockTimer> timerOutTimer = new Queue<CoroutineLockTimer>();
        public long idGenerator;
        public long minTime;
    }
}