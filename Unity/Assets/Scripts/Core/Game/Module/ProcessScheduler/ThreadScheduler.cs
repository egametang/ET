using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    // 一个Process一个固定的线程
    public class ThreadScheduler: Singleton<ThreadScheduler>, ISingletonScheduler
    {
        private bool isStart;
        
        private readonly ConcurrentDictionary<Process, Thread> dictionary = new();

        public void StartScheduler()
        {
            this.isStart = true;
        }

        private void Loop(Process process)
        {
            while (this.isStart)
            {
                try
                {
                    process.LoopOnce();
                    
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void StopScheduler()
        {
            this.isStart = false;
            foreach (var kv in this.dictionary)
            {
                kv.Value.Join();
            }
        }

        public void Add(Process process)
        {
            lock (Game.Instance)
            {
                if (process.Id == 0)
                {
                    return;
                }

                Thread thread = new(()=>this.Loop(process));
                thread.Start();
            }
        }
    }
}