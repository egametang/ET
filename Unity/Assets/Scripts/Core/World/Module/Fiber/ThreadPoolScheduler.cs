using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    internal class ThreadPoolScheduler: IScheduler
    {
        private readonly List<Thread> threads;

        private readonly ConcurrentQueue<int> idQueue = new();
        
        private readonly FiberManager fiberManager;

        public ThreadPoolScheduler(FiberManager fiberManager)
        {
            this.fiberManager = fiberManager;
            int threadCount = Environment.ProcessorCount;
            this.threads = new List<Thread>(threadCount);
            for (int i = 0; i < threadCount; ++i)
            {
                Thread thread = new(this.Loop);
                this.threads.Add(thread);
                thread.Start();
            }
        }

        private void Loop()
        {
            while (true)
            {
                try
                {
                    if (this.fiberManager.IsDisposed())
                    {
                        return;
                    }
                    
                    if (!this.idQueue.TryDequeue(out int id))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    Fiber fiber = this.fiberManager.Get(id);
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
                        SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
                        fiber.Update();
                        fiber.LateUpdate();
                        SynchronizationContext.SetSynchronizationContext(null);
                    }

                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Dispose()
        {
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