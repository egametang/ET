using System.Collections.Generic;

namespace ET
{
    public class MainThreadScheduler: Singleton<MainThreadScheduler>, IVProcessScheduler, ISingletonUpdate, ISingletonLateUpdate
    {
        private readonly Queue<int> idQueue = new();
        private readonly Queue<int> addIds = new();

        public void Update()
        {
            int count = this.idQueue.Count;
            while (count-- == 0)
            {
                if (!this.idQueue.TryDequeue(out int id))
                {
                    continue;
                }

                VProcess vProcess = VProcessSingleton.Instance.Get(id);
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

                VProcess vProcess = VProcessSingleton.Instance.Get(id);
                if (vProcess == null)
                {
                    continue;
                }

                this.idQueue.Enqueue(id);

                vProcess.LateUpdate();
                vProcess.FrameFinishUpdate();
            }

            while (this.addIds.Count > 0)
            {
                this.idQueue.Enqueue(this.addIds.Dequeue());
            }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Add(VProcess vProcess)
        {
            this.addIds.Enqueue(vProcess.Id);
        }
    }
}