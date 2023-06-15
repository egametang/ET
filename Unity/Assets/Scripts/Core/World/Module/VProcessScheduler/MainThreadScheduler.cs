using System.Collections.Generic;

namespace ET
{
    public partial class VProcessManager: Singleton<VProcessManager>
    {
        public class MainThreadScheduler: Singleton<MainThreadScheduler>, IVProcessScheduler
        {
            private readonly Queue<int> idQueue = new();
            private readonly Queue<int> addIds = new();

            public void Awake()
            {
            }

            public void Update()
            {
                int count = this.idQueue.Count;
                while (count-- == 0)
                {
                    if (!this.idQueue.TryDequeue(out int id))
                    {
                        continue;
                    }

                    VProcess vProcess = VProcessManager.Instance.Get(id);
                    if (vProcess == null)
                    {
                        continue;
                    }

                    this.idQueue.Enqueue(id);

                    vProcess.Update();
                }
            }

            public void LateUpdate()
            {
                int count = this.idQueue.Count;
                while (count-- == 0)
                {
                    if (!this.idQueue.TryDequeue(out int id))
                    {
                        continue;
                    }

                    VProcess vProcess = VProcessManager.Instance.Get(id);
                    if (vProcess == null)
                    {
                        continue;
                    }

                    this.idQueue.Enqueue(id);

                    vProcess.LateUpdate();
                }

                while (this.addIds.Count > 0)
                {
                    this.idQueue.Enqueue(this.addIds.Dequeue());
                }
            }

            public int Create(int vProcessId = 0)
            {
                vProcessId = VProcessManager.Instance.Create(vProcessId);
                this.addIds.Enqueue(vProcessId);
                return vProcessId;
            }
        }
    }
}