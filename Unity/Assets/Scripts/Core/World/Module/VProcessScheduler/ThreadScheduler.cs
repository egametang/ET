using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    // 一个Process一个固定的线程
    public class ThreadScheduler: Singleton<ThreadScheduler>, IVProcessScheduler
    {
        private bool isStart;
        
        private readonly ConcurrentDictionary<VProcess, Thread> dictionary = new();

        public void Start()
        {
            this.isStart = true;
        }

        private void Loop(VProcess vProcess)
        {
            while (this.isStart)
            {
                try
                {
                    vProcess.Update();
                    vProcess.LateUpdate();
                    vProcess.FrameFinishUpdate();
                    
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Stop()
        {
            this.isStart = false;
            foreach (var kv in this.dictionary)
            {
                kv.Value.Join();
            }
        }

        public void Add(VProcess vProcess)
        {
            lock (World.Instance)
            {
                if (vProcess.Id == 0)
                {
                    return;
                }

                Thread thread = new(()=>this.Loop(vProcess));
                thread.Start();
            }
        }
    }
}