using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    // 一个Process一个固定的线程
    public class ThreadScheduler: Singleton<ThreadScheduler>, IScheduler
    {
        private readonly ConcurrentDictionary<int, Thread> dictionary = new();

        public void Awake()
        {
        }

        private void Loop(int fiberId)
        {
            Fiber fiber = FiberManager.Instance.Get(fiberId);
            if (fiber == null)
            {
                return;
            }

            while (true)
            {
                try
                {
                    if (this.IsDisposed())
                    {
                        this.dictionary.Remove(fiberId, out _);
                        return;
                    }
                    
                    fiber.Update();
                    fiber.LateUpdate();

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
            foreach (var kv in this.dictionary.ToArray())
            {
                kv.Value.Join();
            }
        }

        public void Add(int fiberId)
        {
            if (this.IsDisposed())
            {
                return;
            }
            
            Thread thread = new(() => this.Loop(fiberId));
            this.dictionary.TryAdd(fiberId, thread);
            thread.Start();
        }
    }
}