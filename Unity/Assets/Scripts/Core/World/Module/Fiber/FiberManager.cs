using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public enum SchedulerType
    {
        Main,
        Thread,
        ThreadPool,
    }
    
    public class FiberManager: Singleton<FiberManager>, ISingletonAwake
    {
        private readonly IScheduler[] schedulers = new IScheduler[3];
        
        private int idGenerator = 10000000; // 10000000以下为保留的用于StartSceneConfig的fiber id, 1个区配置1000个纤程，可以配置10000个区
        private readonly ConcurrentDictionary<int, Fiber> fibers = new();
        
        public void Awake()
        {
            this.schedulers[(int)SchedulerType.Main] = new MainThreadScheduler(this);
            
            //this.schedulers[(int)SchedulerType.Thread] = this.schedulers[(int)SchedulerType.Main];
            //this.schedulers[(int)SchedulerType.ThreadPool] = this.schedulers[(int)SchedulerType.Main];
            
            this.schedulers[(int)SchedulerType.Thread] = new ThreadScheduler(this);
            this.schedulers[(int)SchedulerType.ThreadPool] = new ThreadPoolScheduler(this);
        }
        
        public void Update()
        {
            ((MainThreadScheduler)this.schedulers[0]).Update();
        }

        public void LateUpdate()
        {
            ((MainThreadScheduler)this.schedulers[0]).LateUpdate();
        }

        protected override void Destroy()
        {
            foreach (IScheduler scheduler in this.schedulers)
            {
                scheduler.Dispose();
            }
            
            foreach (var kv in this.fibers)
            {
                kv.Value.Dispose();
            }
        }

        public int CreateFiber(SchedulerType schedulerType, int fiberId, int zone, SceneType sceneType, string name)
        {
            try
            {
                Fiber fiber = new(fiberId, Options.Instance.Process, zone, sceneType, name);
            
                fiber.Root.AddComponent<TimerComponent>();
                fiber.Root.AddComponent<CoroutineLockComponent>();
                fiber.Root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
                fiber.Root.AddComponent<ActorSenderComponent>();
                fiber.Root.AddComponent<ActorRecverComponent>();
            
                // 根据Fiber的SceneType分发Init
                EventSystem.Instance.Invoke((long)sceneType, new FiberInit() {Fiber = fiber});
                
                this.fibers[fiber.Id] = fiber;
                
                this.schedulers[(int) schedulerType].Add(fiberId);
                
                return fiberId;
            }
            catch (Exception e)
            {
                throw new Exception($"create fiber error: {fiberId} {sceneType}", e);
            }
        }
        
        public int CreateFiber(SchedulerType schedulerType, int zone, SceneType sceneType, string name)
        {
            int fiberId = Interlocked.Increment(ref this.idGenerator);
            return this.CreateFiber(schedulerType, fiberId, zone, sceneType, name);
        }
        
        // 不允许外部调用,只能由Schecher执行完成一帧再调用，否则容易出现多线程问题
        public void RemoveFiber(int id)
        {
            if (this.fibers.Remove(id, out Fiber fiber))
            {
                fiber.Dispose();
            }
        }

        // 不允许外部调用，容易出现多线程问题, 只能通过消息通信，不允许直接获取其它Fiber引用
        internal Fiber Get(int id)
        {
            this.fibers.TryGetValue(id, out Fiber fiber);
            return fiber;
        }
    }
}