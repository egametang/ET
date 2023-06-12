using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public class ThreadPoolScheduler: Singleton<ThreadPoolScheduler>, ISingletonScheduler, ISingletonUpdate
    {
        private bool isStart;
        private HashSet<Process> Processes { get; } = new();
        private readonly List<Process> removeProcesses = new();
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new();

        public void StartScheduler()
        {
            this.isStart = true;
        }
        
        public void Update()
        {
            if (!this.isStart)
            {
                return;
            }
            
            this.threadSynchronizationContext.Update();
            
            removeProcesses.Clear();
            foreach (Process process in this.Processes)
            {
                if (process.IsRuning)
                {
                    continue;
                }

                if (process.Id == 0)
                {
                    this.removeProcesses.Add(process);
                }

                process.IsRuning = true;
                ThreadPool.QueueUserWorkItem(process.Loop);
            }

            foreach (Process process in this.removeProcesses)
            {
                this.Processes.Remove(process);
            }
        }

        public void StopScheduler()
        {
            this.isStart = false;
            
            // 等待线程池中的Process Loop完成
            while (true)
            {
                int count = 0;
                
                foreach (Process process in this.Processes)
                {
                    if (process.IsRuning)
                    {
                        break;
                    }

                    ++count;
                }

                if (count == this.Processes.Count)
                {
                    break;
                }
            }
        }

        public void Add(Process process)
        {
            threadSynchronizationContext.Post(()=>this.Processes.Add(process));
        }
    }
}