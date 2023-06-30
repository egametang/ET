using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public class ThreadPoolScheduler: Singleton<ThreadPoolScheduler>, IScheduler, ISingletonAwake<int>
    {
        private bool isStart;

        private readonly HashSet<Thread> threads = new();

        private int ThreadCount { get; set; }

        private readonly ConcurrentQueue<int> idQueue = new();

        public void Awake(int count)
        {
            this.isStart = true;
            this.ThreadCount = count;
            for (int i = 0; i < this.ThreadCount; ++i)
            {
                this.threads.Add(new Thread(this.Loop));
            }

            foreach (Thread thread in this.threads)
            {
                thread.Start();
            }
        }

        private void Loop()
        {
            while (this.isStart)
            {
                try
                {
                    if (!this.idQueue.TryDequeue(out int id))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    Fiber fiber = FiberManager.Instance.Get(id);
                    if (fiber == null)
                    {
                        continue;
                    }

                    if (fiber.IsDisposed)
                    {
                        continue;
                    }
                    
                    this.idQueue.Enqueue(id);

                    // 正在执行的就不执行了
                    if (!fiber.IsRuning)
                    {
                        fiber.Update();
                        fiber.LateUpdate();
                    }

                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        protected override void Destroy()
        {
            this.isStart = false;
            foreach (Thread thread in this.threads)
            {
                thread.Join();
            }
        }

        public void Add(int fiberId)
        {
            this.idQueue.Enqueue(fiberId);
        }
    }
}