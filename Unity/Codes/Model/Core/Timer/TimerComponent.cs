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
    
    [ObjectSystem]
    public class TimerActionAwakeSystem: AwakeSystem<TimerAction, TimerClass, long, int, object>
    {
        public override void Awake(TimerAction self, TimerClass timerClass, long time, int type, object obj)
        {
            self.TimerClass = timerClass;
            self.Object = obj;
            self.Time = time;
            self.Type = type;
        }
    }

    [ObjectSystem]
    public class TimerActionDestroySystem: DestroySystem<TimerAction>
    {
        public override void Destroy(TimerAction self)
        {
            self.Object = null;
            self.Time = 0;
            self.TimerClass = TimerClass.None;
            self.Type = 0;
        }
    }
    
    public class TimerAction: Entity, IAwake, IAwake<TimerClass, long, int, object>, IDestroy
    {
        public TimerClass TimerClass;

        public object Object;

        public long Time;

        public int Type;
    }

    [FriendClass(typeof(TimerAction))]
    [FriendClass(typeof(TimerComponent))]
    public static class TimerComponentSystem
    {
        [ObjectSystem]
        public class TimerComponentAwakeSystem: AwakeSystem<TimerComponent>
        {
            public override void Awake(TimerComponent self)
            {
                TimerComponent.Instance = self;
                self.Init();
            }
        }

        [ObjectSystem]
        public class TimerComponentUpdateSystem: UpdateSystem<TimerComponent>
        {
            public override void Update(TimerComponent self)
            {
                #region 每帧执行的timer，不用foreach TimeId，减少GC

                int count = self.everyFrameTimer.Count;
                for (int i = 0; i < count; ++i)
                {
                    long timerId = self.everyFrameTimer.Dequeue();
                    TimerAction timerAction = self.GetChild<TimerAction>(timerId);
                    if (timerAction == null)
                    {
                        continue;
                    }
                    self.Run(timerAction);
                }

                #endregion
            
                if (self.TimeId.Count == 0)
                {
                    return;
                }

                self.timeNow = TimeHelper.ServerNow();

                if (self.timeNow < self.minTime)
                {
                    return;
                }

                self.TimeId.ForEachFunc(self.foreachFunc);

                while (self.timeOutTime.Count > 0)
                {
                    long time = self.timeOutTime.Dequeue();
                    var list = self.TimeId[time];
                    for (int i = 0; i < list.Count; ++i)
                    {
                        long timerId = list[i];
                        self.timeOutTimerIds.Enqueue(timerId);
                    }

                    self.TimeId.Remove(time);
                }

                while (self.timeOutTimerIds.Count > 0)
                {
                    long timerId = self.timeOutTimerIds.Dequeue();

                    TimerAction timerAction = self.GetChild<TimerAction>(timerId);
                    if (timerAction == null)
                    {
                        continue;
                    }
                    self.Run(timerAction);
                }
            }
        }
    
        [ObjectSystem]
        public class TimerComponentLoadSystem: LoadSystem<TimerComponent>
        {
            public override void Load(TimerComponent self)
            {
                self.Init();
            }
        }

        [ObjectSystem]
        public class TimerComponentDestroySystem: DestroySystem<TimerComponent>
        {
            public override void Destroy(TimerComponent self)
            {
                TimerComponent.Instance = null;
            }
        }

        private static void Init(this TimerComponent self)
        {
            self.foreachFunc = (k, v) =>
            {
                if (k > self.timeNow)
                {
                    self.minTime = k;
                    return false;
                }

                self.timeOutTime.Enqueue(k);
                return true;
            };
            
            self.timerActions = new ITimer[TimerComponent.TimeTypeMax];

            List<Type> types = Game.EventSystem.GetTypes(typeof (TimerAttribute));

            foreach (Type type in types)
            {
                ITimer iTimer = Activator.CreateInstance(type) as ITimer;
                if (iTimer == null)
                {
                    Log.Error($"Timer Action {type.Name} 需要继承 ITimer");
                    continue;
                }
                
                object[] attrs = type.GetCustomAttributes(typeof(TimerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                foreach (object attr in attrs)
                {
                    TimerAttribute timerAttribute = attr as TimerAttribute;
                    self.timerActions[timerAttribute.Type] = iTimer;
                }
            }
        }

        private static void Run(this TimerComponent self, TimerAction timerAction)
        {
            switch (timerAction.TimerClass)
            {
                case TimerClass.OnceTimer:
                {
                    int type = timerAction.Type;
                    ITimer timer = self.timerActions[type];
                    if (timer == null)
                    {
                        Log.Error($"not found timer action: {type}");
                        return;
                    }
                    object obj = timerAction.Object;
                    self.Remove(timerAction.Id);
                    timer.Handle(obj);
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    ETTask<bool> tcs = timerAction.Object as ETTask<bool>;
                    self.Remove(timerAction.Id);
                    tcs.SetResult(true);
                    break;
                }
                case TimerClass.RepeatedTimer:
                {
                    int type = timerAction.Type;
                    long tillTime = TimeHelper.ServerNow() + timerAction.Time;
                    self.AddTimer(tillTime, timerAction);

                    ITimer timer = self.timerActions[type];
                    if (timer == null)
                    {
                        Log.Error($"not found timer action: {type}");
                        return;
                    }
                    timer.Handle(timerAction.Object);
                    break;
                }
            }
        }
        
        private static void AddTimer(this TimerComponent self, long tillTime, TimerAction timer)
        {
            if (timer.TimerClass == TimerClass.RepeatedTimer && timer.Time == 0)
            {
                self.everyFrameTimer.Enqueue(timer.Id);
                return;
            }
            self.TimeId.Add(tillTime, timer.Id);
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

            TimerAction timerAction = self.GetChild<TimerAction>(id);
            if (timerAction == null)
            {
                return false;
            }
            timerAction.Dispose();
            return true;
        }

        public static async ETTask<bool> WaitTillAsync(this TimerComponent self, long tillTime, ETCancellationToken cancellationToken = null)
        {
            if (self.timeNow >= tillTime)
            {
                return true;
            }

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            TimerAction timer = self.AddChild<TimerAction, TimerClass, long, int, object>(TimerClass.OnceWaitTimer, tillTime - self.timeNow, 0, tcs, true);
            self.AddTimer(tillTime, timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (self.Remove(timerId))
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

        public static async ETTask<bool> WaitFrameAsync(this TimerComponent self, ETCancellationToken cancellationToken = null)
        {
            bool ret = await self.WaitAsync(1, cancellationToken);
            return ret;
        }

        public static async ETTask<bool> WaitAsync(this TimerComponent self, long time, ETCancellationToken cancellationToken = null)
        {
            if (time == 0)
            {
                return true;
            }
            long tillTime = TimeHelper.ServerNow() + time;

            ETTask<bool> tcs = ETTask<bool>.Create(true);
            
            TimerAction timer = self.AddChild<TimerAction, TimerClass, long, int, object>(TimerClass.OnceWaitTimer, time, 0, tcs, true);
            self.AddTimer(tillTime, timer);
            long timerId = timer.Id;

            void CancelAction()
            {
                if (self.Remove(timerId))
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
        public static long NewOnceTimer(this TimerComponent self, long tillTime, int type, object args)
        {
            if (tillTime < TimeHelper.ServerNow())
            {
                Log.Warning($"new once time too small: {tillTime}");
            }
            TimerAction timer = self.AddChild<TimerAction, TimerClass, long, int, object>(TimerClass.OnceTimer, tillTime, type, args, true);
            self.AddTimer(tillTime, timer);
            return timer.Id;
        }

        public static long NewFrameTimer(this TimerComponent self, int type, object args)
        {
#if NOT_UNITY
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
#if NOT_UNITY
			if (time < 100)
			{ 
				throw new Exception($"repeated timer < 100, timerType: time: {time}");
			}
#endif
            long tillTime = TimeHelper.ServerNow() + time;
            TimerAction timer = self.AddChild<TimerAction, TimerClass, long, int, object>(TimerClass.RepeatedTimer, time, type, args, true);

            // 每帧执行的不用加到timerId中，防止遍历
            self.AddTimer(tillTime, timer);
            return timer.Id;
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

    

    [ComponentOf(typeof(Scene))]
    public class TimerComponent: Entity, IAwake, IUpdate, ILoad, IDestroy
    {
        public static TimerComponent Instance
        {
            get;
            set;
        }
        
        public long timeNow;
        public Func<long, List<long>, bool> foreachFunc;

        /// <summary>
        /// key: time, value: timer id
        /// </summary>
        public readonly MultiMap<long, long> TimeId = new MultiMap<long, long>();

        public readonly Queue<long> timeOutTime = new Queue<long>();

        public readonly Queue<long> timeOutTimerIds = new Queue<long>();
        
        public readonly Queue<long> everyFrameTimer = new Queue<long>();

        // 记录最小时间，不用每次都去MultiMap取第一个值
        public long minTime;

        public const int TimeTypeMax = 10000;

        public ITimer[] timerActions;
    }
}