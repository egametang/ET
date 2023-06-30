using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    // 一个Process一个固定的线程
    public class ThreadScheduler: Singleton<ThreadScheduler>, IScheduler
    {
        private bool isStart;

        private readonly ConcurrentDictionary<int, Thread> dictionary = new();

        public void Awake()
        {
            this.isStart = true;
        }

        private void Loop(int fiberId)
        {
            Fiber fiber = FiberManager.Instance.Get(fiberId);
            if (fiber == null)
            {
                return;
            }

            while (this.isStart)
            {
                try
                {
                    if (fiber.IsDisposed)
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
            this.isStart = false;
            foreach (var kv in this.dictionary)
            {
                kv.Value.Join();
            }
        }

        public void Add(int fiberId)
        {
            Thread thread = new(() => this.Loop(fiberId));
            thread.Start();
        }
    }
}