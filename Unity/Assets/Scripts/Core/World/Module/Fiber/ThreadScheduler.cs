using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    // 一个Fiber一个固定的线程
    internal class ThreadScheduler: IScheduler
    {
        private readonly ConcurrentDictionary<int, Thread> dictionary = new();
        
        private readonly FiberManager fiberManager;

        public ThreadScheduler(FiberManager fiberManager)
        {
            this.fiberManager = fiberManager;
        }

        private void Loop(int fiberId)
        {
            Fiber fiber = fiberManager.Get(fiberId);
            Fiber.Instance = fiber;
            SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
            
            while (true)
            {
                if (this.fiberManager.IsDisposed())
                {
                    return;
                }
                
                fiber = fiberManager.Get(fiberId);
                if (fiber == null)
                {
                    this.dictionary.Remove(fiberId, out _);
                    return;
                }
                
                if (fiber.IsDisposed)
                {
                    this.dictionary.Remove(fiberId, out _);
                    return;
                }
                
                fiber.Update();
                fiber.LateUpdate();

                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            foreach (var kv in this.dictionary.ToArray())
            {
                kv.Value.Join();
            }
        }

        public void Add(int fiberId)
        {
            Thread thread = new(() => this.Loop(fiberId));
            this.dictionary.TryAdd(fiberId, thread);
            thread.Start();
        }
    }
}