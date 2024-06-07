using System;
using System.Collections;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(TimerComponent))]
    public static partial class TimerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TimerComponent self)
        {
        }
        
        [EntitySystem]
        private static void Update(this TimerComponent self)
        {
            if (self.timeId.Count == 0)
            {
                return;
            }

            long timeNow = self.GetNow();

            if (timeNow < self.minTime)
            {
                return;
            }

            foreach (var kv in self.timeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    self.minTime = k;
                    break;
                }

                self.timeOutTime.Enqueue(k);
            }

            while (self.timeOutTime.Count > 0)
            {
                long time = self.timeOutTime.Dequeue();
                List<long> list = self.timeId[time];
                for (int i = 0; i < list.Count; ++i)
                {
                    long timerId = list[i];
                    self.timeOutTimerIds.Enqueue(timerId);
                }
                self.timeId.Remove(time);
            }

            if (self.timeId.Count == 0)
            {
                self.minTime = long.MaxValue;
            }

            while (self.timeOutTimerIds.Count > 0)
            {
                long timerId = self.timeOutTimerIds.Dequeue();

                if (!self.timerActions.Remove(timerId, out TimerAction timerAction))
                {
                    continue;
                }
                
                self.Run(timerId, ref timerAction);
            }
        }
        
        private static long GetId(this TimerComponent self)
        {
            return ++self.idGenerator;
        }

        private static long GetNow(this TimerComponent self)
        {
            return TimeInfo.Instance.ServerFrameTime();
        }

        private static void Run(this TimerComponent self, long timerId, ref TimerAction timerAction)
        {
            switch (timerAction.TimerClass)
            {
                case TimerClass.OnceTimer:
                {
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    ETTask tcs = timerAction.Object as ETTask;
                    tcs.SetResult();
                    break;
                }
                case TimerClass.RepeatedTimer:
                {                    
                    long timeNow = self.GetNow();
                    timerAction.StartTime = timeNow;
                    self.AddTimer(timerId, ref timerAction);
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    break;
                }
            }
        }

        private static void AddTimer(this TimerComponent self, long timerId, ref TimerAction timer)
        {
            long tillTime = timer.StartTime + timer.Time;
            self.timeId.Add(tillTime, timerId);
            self.timerActions.Add(timerId, timer);
            if (tillTime < self.minTime)
            {
                self.minTime = tillTime;
            }
        }

        public static bool Remove(this TimerComponent self, ref long id)
        {
            long i = id;
            id = 0;
            return self.Remove(i);
        }

        private static bool Remove(this TimerComponent self, long id)
        {
            if (id == 0)
            {
                return false;
            }

            if (!self.timerActions.Remove(id, out TimerAction _))
            {
                return false;
            }
            return true;
        }

        public static async ETTask WaitTillAsync(this TimerComponent self, long tillTime)
        {
            long timeNow = self.GetNow();
            if (timeNow >= tillTime)
            {
                return;
            }

            ETTask tcs = ETTask.Create(true);
            long timerId = self.GetId();
            TimerAction timer = new(TimerClass.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            self.AddTimer(timerId, ref timer);

            void CancelAction()
            {
                if (self.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

            ETCancellationToken cancellationToken = await ETTaskHelper.GetContextAsync<ETCancellationToken>();
            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }

        public static async ETTask WaitFrameAsync(this TimerComponent self)
        {
            await self.WaitAsync(1);
        }

        public static async ETTask WaitAsync(this TimerComponent self, long time)
        {
            if (time == 0)
            {
                return;
            }

            long timeNow = self.GetNow();

            ETTask tcs = ETTask.Create(true);
            long timerId = self.GetId();
            TimerAction timer = new (TimerClass.OnceWaitTimer, timeNow, time, 0, tcs);
            self.AddTimer(timerId, ref timer);

            void CancelAction()
            {
                if (self.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

            ETCancellationToken cancellationToken = await ETTaskHelper.GetContextAsync<ETCancellationToken>();
            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }

        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public static long NewOnceTimer(this TimerComponent self, long tillTime, int type, object args)
        {
            long timeNow = self.GetNow();
            if (tillTime < timeNow)
            {
                Log.Error($"new once time too small: {tillTime}");
            }
            long timerId = self.GetId();
            TimerAction timer = new (TimerClass.OnceTimer, timeNow, tillTime - timeNow, type, args);
            self.AddTimer(timerId, ref timer);
            return timerId;
        }

        public static long NewFrameTimer(this TimerComponent self, int type, object args)
        {
#if DOTNET
            return self.NewRepeatedTimerInner(100, type, args);
#else
            return self.NewRepeatedTimerInner(0, type, args);
#endif
        }

        /// <summary>
        /// 创建一个RepeatedTimer
        /// </summary>
        private static long NewRepeatedTimerInner(this TimerComponent self, long time, int type, object args)
        {
#if DOTNET
            if (time < 100)
            {
                throw new Exception($"repeated timer < 100, timerType: time: {time}");
            }
#endif
            
            long timeNow = self.GetNow();
            long timerId = self.GetId();
            TimerAction timer = new (TimerClass.RepeatedTimer, timeNow, time, type, args);

            // 每帧执行的不用加到timerId中，防止遍历
            self.AddTimer(timerId, ref timer);
            return timerId;
        }

        public static long NewRepeatedTimer(this TimerComponent self, long time, int type, object args)
        {
            if (time < 100)
            {
                Log.Error($"time too small: {time}");
                return 0;
            }

            return self.NewRepeatedTimerInner(time, type, args);
        }
    }
}
