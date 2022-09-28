using System.Collections.Generic;

namespace ET
{
    public enum TimerClass
    {
        None,
        OnceTimer,
        OnceWaitTimer,
        RepeatedTimer,
    }

    public class TimerAction
    {
        public static TimerAction Create(long id, TimerClass timerClass, long startTime, long time, int type, object obj)
        {
            TimerAction timerAction = ObjectPool.Instance.Fetch<TimerAction>();
            timerAction.Id = id;
            timerAction.TimerClass = timerClass;
            timerAction.StartTime = startTime;
            timerAction.Object = obj;
            timerAction.Time = time;
            timerAction.Type = type;
            return timerAction;
        }

        public long Id;
        
        public TimerClass TimerClass;

        public object Object;

        public long StartTime;

        public long Time;

        public int Type;
        
        public void Recycle()
        {
            this.Id = 0;
            this.Object = null;
            this.StartTime = 0;
            this.Time = 0;
            this.TimerClass = TimerClass.None;
            this.Type = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }

    public struct TimerCallback
    {
        public object Args;
    }

    public class TimerComponent: Singleton<TimerComponent>, ISingletonUpdate
    {
        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        private readonly MultiMap<long, long> TimeId = new MultiMap<long, long>();

        private readonly Queue<long> timeOutTime = new Queue<long>();

        private readonly Queue<long> timeOutTimerIds = new Queue<long>();

        private readonly Dictionary<long, TimerAction> timerActions = new Dictionary<long, TimerAction>();

        private long idGenerator;

        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime = long.MaxValue;

        private long GetId()
        {
            return ++this.idGenerator;
        }

        private static long GetNow()
        {
            return TimeHelper.ClientFrameTime();
        }

        public void Update()
        {
            if (this.TimeId.Count == 0)
            {
                return;
            }

            long timeNow = GetNow();

            if (timeNow < this.minTime)
            {
                return;
            }

            foreach (KeyValuePair<long, List<long>> kv in this.TimeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    this.minTime = k;
                    break;
                }

                this.timeOutTime.Enqueue(k);
            }

            while (this.timeOutTime.Count > 0)
            {
                long time = this.timeOutTime.Dequeue();
                var list = this.TimeId[time];
                for (int i = 0; i < list.Count; ++i)
                {
                    long timerId = list[i];
                    this.timeOutTimerIds.Enqueue(timerId);
                }
                this.TimeId.Remove(time);
            }

            while (this.timeOutTimerIds.Count > 0)
            {
                long timerId = this.timeOutTimerIds.Dequeue();

                if (!this.timerActions.Remove(timerId, out TimerAction timerAction))
                {
                    continue;
                }
                
                this.Run(timerAction);
            }
        }

        private void Run(TimerAction timerAction)
        {
            switch (timerAction.TimerClass)
            {
                case TimerClass.OnceTimer:
                {
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    ETTask<bool> tcs = timerAction.Object as ETTask<bool>;
                    tcs.SetResult(true);
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.RepeatedTimer:
                {                    
                    long timeNow = GetNow();
                    timerAction.StartTime = timeNow;
                    this.AddTimer(timerAction);
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    break;
                }
            }
        }

        private void AddTimer(TimerAction timer)
        {
            long tillTime = timer.StartTime + timer.Time;
            this.TimeId.Add(tillTime, timer.Id);
            this.timerActions.Add(timer.Id, timer);
            if (tillTime < this.minTime)
            {
                this.minTime = tillTime;
            }
        }

        public bool Remove(ref long id)
        {
            long i = id;
            id = 0;
            return this.Remove(i);
        }

        private bool Remove(long id)
        {
            if (id == 0)
            {
                return false;
            }

            if (!this.timerActions.Remove(id, out TimerAction timerAction))
            {
                return false;
            }
            timerAction.Recycle();
            return true;
        }

        public async ETTask<bool> WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null)
        {
            long timeNow = GetNow();
            if (timeNow >= tillTime)
            {
                return true;
            }

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult(false);
                }
            }

            bool ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            return ret;
        }

        public async ETTask<bool> WaitFrameAsync(ETCancellationToken cancellationToken = null)
        {
            bool ret = await this.WaitAsync(1, cancellationToken);
            return ret;
        }

        public async ETTask<bool> WaitAsync(long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0)
            {
                return true;
            }

            long timeNow = GetNow();

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, time, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult(false);
                }
            }

            bool ret;
            try
            {
                cancellationToken?.Add(CancelAction);
                ret = await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }

            return ret;
        }

        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public long NewOnceTimer(long tillTime, int type, object args)
        {
            long timeNow = GetNow();
            if (tillTime < timeNow)
            {
                Log.Error($"new once time too small: {tillTime}");
            }

            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceTimer, timeNow, tillTime - timeNow, type, args);
            this.AddTimer(timer);
            return timer.Id;
        }

        public long NewFrameTimer(int type, object args)
        {
#if DOTNET
            return this.NewRepeatedTimerInner(100, type, args);
#else
            return this.NewRepeatedTimerInner(0, type, args);
#endif
        }

        /// <summary>
        /// 创建一个RepeatedTimer
        /// </summary>
        private long NewRepeatedTimerInner(long time, int type, object args)
        {
#if DOTNET
            if (time < 100)
            {
                throw new Exception($"repeated timer < 100, timerType: time: {time}");
            }
#endif
            
            long timeNow = GetNow();
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.RepeatedTimer, timeNow, time, type, args);

            // 每帧执行的不用加到timerId中，防止遍历
            this.AddTimer(timer);
            return timer.Id;
        }

        public long NewRepeatedTimer(long time, int type, object args)
        {
            if (time < 100)
            {
                Log.Error($"time too small: {time}");
                return 0;
            }

            return this.NewRepeatedTimerInner(time, type, args);
        }
    }
}