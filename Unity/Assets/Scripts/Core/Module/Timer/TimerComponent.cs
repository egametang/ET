using System.Collections.Generic;

namespace ET
{
    // TimerClass是一个枚举类型，表示定时器的类别
    public enum TimerClass
    {
        None,
        OnceTimer,       // 一次性定时器，到达指定时间后执行一次事件
        OnceWaitTimer,   // 一次性等待定时器，到达指定时间后完成一个ETTask
        RepeatedTimer,   // 重复性定时器，每隔指定时间执行一次事件
    }

    // 定时器的行为
    public class TimerAction
    {
        // Create方法是一个静态方法，用于从对象池中获取或创建一个TimerAction对象，并设置其属性
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

        /// <summary>
        ///  定时器的唯一标识
        /// </summary>
        public long Id;

        /// <summary>
        /// 定时器的类别
        /// </summary>
        public TimerClass TimerClass;

        /// <summary>
        /// 定时器的参数或任务
        /// </summary>
        public object Object;

        /// <summary>
        /// 定时器的开始时间
        /// </summary>
        public long StartTime;

        /// <summary>
        /// 定时器的间隔时间或结束时间
        /// </summary>
        public long Time;

        /// <summary>
        /// 定时器的事件类型或调用类型
        /// </summary>
        public int Type;

        /// <summary>
        /// Recycle方法用于重置定时器的属性，并将其回收到对象池中
        /// </summary>
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

    // 定时器的回调参数
    public struct TimerCallback
    {
        public object Args;
    }

    /// <summary>
    /// 用于管理所有的定时器
    /// </summary>
    public class TimerComponent: Singleton<TimerComponent>, ISingletonUpdate
    {
        /// <summary>
        /// key: time, value: timer id
        /// TimeId是一个多映射，将一个时间点映射到一组定时器id
        /// </summary>
        private readonly MultiMap<long, long> TimeId = new();

        /// <summary>
        /// 用于存储超时的时间点
        /// </summary>
        private readonly Queue<long> timeOutTime = new();

        /// <summary>
        /// 用于存储超时的定时器id
        /// </summary>
        private readonly Queue<long> timeOutTimerIds = new();

        /// <summary>
        /// 将一个定时器id映射到一个TimerAction对象
        /// </summary>
        private readonly Dictionary<long, TimerAction> timerActions = new();

        // 用于生成唯一的定时器id
        private long idGenerator;

        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime = long.MaxValue;

        // GetId方法返回下一个可用的定时器id，并自增idGenerator
        private long GetId()
        {
            return ++this.idGenerator;
        }

        // 返回当前的客户端帧时间
        private static long GetNow()
        {
            return TimeHelper.ClientFrameTime();
        }

        // Update方法在每帧调用，检查并执行超时的定时器
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

            // 遍历TimeId，将超时的时间点加入到timeOutTime队列中
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

            // 遍历timeOutTime队列，将对应的定时器id加入到timeOutTimerIds队列中，并从TimeId中移除
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
                    ETTask tcs = timerAction.Object as ETTask;
                    tcs.SetResult();
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

        public async ETTask WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null)
        {
            long timeNow = GetNow();
            if (timeNow >= tillTime)
            {
                return;
            }

            ETTask tcs = ETTask.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

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

        public async ETTask WaitFrameAsync(ETCancellationToken cancellationToken = null)
        {
            await this.WaitAsync(1, cancellationToken);
        }

        public async ETTask WaitAsync(long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0)
            {
                return;
            }

            long timeNow = GetNow();

            ETTask tcs = ETTask.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, time, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (this.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

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