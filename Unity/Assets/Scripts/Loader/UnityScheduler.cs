using System.Collections.Generic;

namespace ET
{
    public class UnityScheduler: Singleton<UnityScheduler>, ISingletonScheduler
    {
        public bool IsStart;

        private Process process;

        public void StartScheduler()
        {
            Init.Instance.IsStart = true;
        }

        public void StopScheduler()
        {
            Init.Instance.ThreadSynchronizationContext.Post(() => { Init.Instance.IsStart = false; });
            
            // Process Loop完成
            while (true)
            {
                if (process.IsRuning)
                {
                    break;
                }
            }
        }

        public void Add(Process process)
        {
            lock (Game.Instance)
            {
                this.process = process;
                Init.Instance.ThreadSynchronizationContext.Post(() => { Init.Instance.Process = process; });
            }
        }
    }
}