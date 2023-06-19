using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public partial class VProcessManager: Singleton<VProcessManager>
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

                        VProcess vProcess = VProcessManager.Instance.Get(id);
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

            public override void Dispose()
            {
                base.Dispose();
                
                this.isStart = false;
                foreach (Thread thread in this.threads)
                {
                    thread.Join();
                }
            }

            public void Add(int vProcessId)
            {
                this.idQueue.Enqueue(vProcessId);
            }
        }
    }
}