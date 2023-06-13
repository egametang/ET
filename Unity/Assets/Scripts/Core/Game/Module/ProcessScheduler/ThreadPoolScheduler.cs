using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public class ThreadPoolScheduler: Singleton<ThreadPoolScheduler>, ISingletonScheduler, ISingletonUpdate
    {
        private bool isStart;
        private Dictionary<int, Process> Processes { get; } = new();
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
            
            foreach ((int _, Process process) in this.Processes)
            {
                if (process.IsRuning)
                {
                    continue;
                }

                process.IsRuning = true;
                ThreadPool.QueueUserWorkItem(process.Loop);
            }
        }

        public void StopScheduler()
        {
            this.isStart = false;
            
            // 等待线程池中的Process Loop完成
            while (true)
            {
                int count = 0;
                
                foreach ((int _, Process process) in this.Processes)
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
            lock (Game.Instance)
            {
                this.Processes.Add(process.Id, process);
            }
        }

        public void Remove(Process process)
        {
            lock (Game.Instance)
            {
                this.Processes.Remove(process.Id);
            }
        }
    }
}