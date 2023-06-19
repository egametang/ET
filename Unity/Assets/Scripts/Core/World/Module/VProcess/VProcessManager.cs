using System.Collections.Generic;

namespace ET
{
    public partial class VProcessManager: Singleton<VProcessManager>
    {
        private int idGenerator = int.MaxValue;
        private readonly Dictionary<int, VProcess> vProcesses = new();
        
        public int Create(int processId = 0)
        {
            lock (this)
            {
                if (processId == 0)
                {
                    processId = --this.idGenerator;
                }
                VProcess vProcess = new(Options.Instance.Process, processId);
                vProcess.AddSingleton<Root>();
                vProcess.AddSingleton<EntitySystem>();
                this.vProcesses.TryAdd(vProcess.Id, vProcess);
                return processId;
            }
        }
        
        // 不允许外部调用,只能由Schecher执行完成一帧调用，否则容易出现多线程问题
        private void Remove(int id)
        {
            lock (this)
            {
                if (this.vProcesses.Remove(id, out VProcess process))
                {
                    process.Dispose();
                }
            }
        }

        // 不允许外部调用，容易出现多线程问题
        private VProcess Get(int id)
        {
            lock (this)
            {
                this.vProcesses.TryGetValue(id, out VProcess process);
                return process;
            }
        }
    }
}