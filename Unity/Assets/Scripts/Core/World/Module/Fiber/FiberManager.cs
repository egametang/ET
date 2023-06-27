using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public partial class FiberManager: Singleton<FiberManager>, ISingletonAwake
    {
        private int idGenerator = 1000; // 1000以下为保留的fiber id
        private readonly ConcurrentDictionary<int, Fiber> fibers = new();
        
        public void Awake()
        {
        }
        
        public int Create(int fiberId, SceneType sceneType)
        {
            fiberId = Interlocked.Increment(ref this.idGenerator);
            Fiber fiber = new(fiberId, Options.Instance.Process, sceneType);
            
            fiber.AddComponent<TimerComponent>();
            fiber.AddComponent<CoroutineLockComponent>();
            fiber.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderMessage);
                
            // 根据Fiber的SceneType分发Init
            EventSystem.Instance.Invoke((int)sceneType, new FiberInit() {Fiber = fiber});
                
            this.fibers[(int)fiber.Id] = fiber;
            return fiberId;
        }
        
        // 不允许外部调用,只能由Schecher执行完成一帧调用，否则容易出现多线程问题
        private void Remove(int id)
        {
            if (this.fibers.Remove(id, out Fiber fiber))
            {
                fiber.Dispose();
            }
        }

        // 不允许外部调用，容易出现多线程问题
        private Fiber Get(int id)
        {
            this.fibers.TryGetValue(id, out Fiber fiber);
            return fiber;
        }
    }
}