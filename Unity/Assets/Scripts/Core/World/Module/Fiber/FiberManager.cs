using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public struct FiberInit
    {
        public SceneType SceneType;
    }

    public partial class FiberManager: Singleton<FiberManager>, ISingletonAwake
    {
        private int idGenerator = int.MaxValue;
        private readonly Dictionary<int, Fiber> fibers = new();
        
        public void Awake()
        {
        }
        
        public int Create(int fiberId, SceneType sceneType)
        {
            lock (this)
            {
                if (fiberId == 0)
                {
                    fiberId = --this.idGenerator;
                }
                Fiber fiber = new(fiberId, Options.Instance.Process, sceneType);
                this.fibers.Add((int)fiber.Id, fiber);
                EventSystem.Instance.Invoke((int)sceneType, new FiberInit());
                return fiberId;
            }
        }
        
        // 不允许外部调用,只能由Schecher执行完成一帧调用，否则容易出现多线程问题
        private void Remove(int id)
        {
            lock (this)
            {
                if (this.fibers.Remove(id, out Fiber process))
                {
                    process.Dispose();
                }
            }
        }

        // 不允许外部调用，容易出现多线程问题
        private Fiber Get(int id)
        {
            lock (this)
            {
                this.fibers.TryGetValue(id, out Fiber process);
                return process;
            }
        }
    }
}