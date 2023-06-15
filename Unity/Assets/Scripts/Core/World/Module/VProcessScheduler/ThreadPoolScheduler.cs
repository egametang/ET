using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public class ThreadPoolScheduler: Singleton<ThreadPoolScheduler>, IVProcessScheduler
    {
        private bool isStart;
        
        private readonly HashSet<Thread> threads = new();
        
        public int ThreadCount { get; set; }

        private readonly ConcurrentQueue<int> idQueue = new();

        public void Start()
        {
            this.isStart = true;
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

                    VProcess vProcess = VProcessSingleton.Instance.Get(id);
                    if (vProcess == null)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    // 执行过的或者正在执行的进程放到队尾
                    if (vProcess.IsRuning)
                    {
                        vProcess.Update();
                        vProcess.LateUpdate();
                        vProcess.FrameFinishUpdate();
                    }

                    this.idQueue.Enqueue(id);
                    
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
            foreach (Thread thread in this.threads)
            {
                thread.Join();
            }
        }

        public void Add(VProcess vProcess)
        {
            lock (World.Instance)
            {
                int id = vProcess.Id;
                if (id == 0)
                {
                    return;
                }
                this.idQueue.Enqueue(id);
            }
        }
    }
}