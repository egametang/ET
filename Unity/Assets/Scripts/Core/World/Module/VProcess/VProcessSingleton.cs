using System.Collections.Generic;

namespace ET
{
    public class VProcessSingleton: Singleton<VProcessSingleton>
    {
        private int idGenerator;
        private readonly Dictionary<int, VProcess> vProcesses = new();
        
        public VProcess Create()
        {
            lock (this)
            {
                int id = ++this.idGenerator;
                VProcess vProcess = new(id);
                this.vProcesses.TryAdd(vProcess.Id, vProcess);
                return vProcess;
            }
        }
        
        public void Remove(int id)
        {
            lock (this)
            {
                if (this.vProcesses.Remove(id, out VProcess process))
                {
                    process.Dispose();
                }
            }
        }

        public VProcess Get(int id)
        {
            lock (this)
            {
                this.vProcesses.TryGetValue(id, out VProcess process);
                return process;
            }
        }
    }
}