using System;
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

    public class TimerAction: IDisposable
    {
        public static TimerAction Create(TimerClass timerClass, long time, int type, object obj)
        {
            TimerAction timerAction = ObjectPool.Instance.Fetch<TimerAction>();
            timerAction.TimerClass = timerClass;
            timerAction.Object = obj;
            timerAction.Time = time;
            timerAction.Type = type;
            return timerAction;
        }

        public long Id;
        
        public TimerClass TimerClass;

        public object Object;

        public long Time;

        public int Type;
        
        public void Dispose()
        {
            this.Object = null;
            this.Time = 0;
            this.TimerClass = TimerClass.None;
            this.Type = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }

    public struct TimerCallback: ICallback
    {
        public int Id { get; set; }
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

        public readonly Queue<long> everyFrameTimer = new Queue<long>();

        private readonly Dictionary<long, TimerAction> timerActions = new Dictionary<long, TimerAction>();

        private long idGenerator;

        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime;

        private long GetId()
        {
            return ++this.idGenerator;
        }

        public void Update()
        {
            if (this.TimeId.Count == 0)
            {
                return;
            }

            long timeNow = TimeHelper.ClientNow();

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

                this.timerActions.TryGetValue(timerId, out TimerAction timerAction);
                if (timerAction == null)
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
                    int type = timerAction.Type;
                    EventSystem.Instance.Callback(new TimerCallback() { Id = type, Args = timerAction.Object });
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    ETTask<bool> tcs = timerAction.Object as ETTask<bool>;
                    this.Remove(timerAction.Id);
                    tcs.SetResult(true);
                    break;
                }
                case TimerClass.RepeatedTimer:
                {
                    int type = timerAction.Type;
                    long tillTime = TimeHelper.ClientNow() + timerAction.Time;
                    this.AddTimer(tillTime, timerAction);
                    EventSystem.Instance.Callback(new TimerCallback() { Id = type, Args = timerAction.Object });
                    break;
                }
            }
        }

        private void AddTimer(long tillTime, TimerAction timer)
        {
            timer.Id = this.GetId();
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

            this.timerActions.TryGetValue(id, out TimerAction timerAction);
            if (timerAction == null)
            {
                return false;
            }

            timerAction.Dispose();
            return true;
        }

        public async ETTask<bool> WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null)
        {
            long timeNow = TimeHelper.ClientNow();
            if (timeNow >= tillTime)
            {
                return true;
            }

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = TimerAction.Create(TimerClass.OnceWaitTimer, tillTime - timeNow, 0, tcs);
            this.AddTimer(tillTime, timer);
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

            long tillTime = TimeHelper.ClientNow() + time;

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = TimerAction.Create(TimerClass.OnceWaitTimer, time, 0, tcs);
            this.AddTimer(tillTime, timer);
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
            if (tillTime < TimeHelper.ClientNow())
            {
                Log.Warning($"new once time too small: {tillTime}");
            }

            TimerAction timer = TimerAction.Create(TimerClass.OnceTimer, tillTime, type, args);
            this.AddTimer(tillTime, timer);
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
            long tillTime = TimeHelper.ClientNow() + time;
            TimerAction timer = TimerAction.Create(TimerClass.RepeatedTimer, time, type, args);

            // 每帧执行的不用加到timerId中，防止遍历
            this.AddTimer(tillTime, timer);
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