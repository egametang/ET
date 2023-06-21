using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public partial class VProcessManager: Singleton<VProcessManager>, ISingletonAwake
    {
        private int idGenerator = int.MaxValue;
        private readonly Dictionary<int, VProcess> vProcesses = new();
        
        public void Awake()
        {
        }
        
        public int Create(int processId, SceneType sceneType)
        {
            lock (this)
            {
                if (processId == 0)
                {
                    processId = --this.idGenerator;
                }
                VProcess vProcess = new(processId, Options.Instance.Process, sceneType);
                vProcess.AddComponent<VProcessActor>();
                this.vProcesses.Add((int)vProcess.Id, vProcess);
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