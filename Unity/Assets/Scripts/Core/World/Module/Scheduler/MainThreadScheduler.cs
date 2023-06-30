using System.Collections.Generic;

namespace ET
{
    public class MainThreadScheduler: Singleton<MainThreadScheduler>, IScheduler, ISingletonAwake
    {
        private readonly Queue<int> idQueue = new();
        private readonly Queue<int> addIds = new();

        public void Awake()
        {
        }

        public void Update()
        {
            int count = this.idQueue.Count;
            while (count-- > 0)
            {
                if (!this.idQueue.TryDequeue(out int id))
                {
                    continue;
                }

                Fiber fiber = FiberManager.Instance.Get(id);
                if (fiber == null)
                {
                    continue;
                }

                this.idQueue.Enqueue(id);

                fiber.Update();
            }
        }

        public void LateUpdate()
        {
            int count = this.idQueue.Count;
            while (count-- > 0)
            {
                if (!this.idQueue.TryDequeue(out int id))
                {
                    continue;
                }

                Fiber fiber = FiberManager.Instance.Get(id);
                if (fiber == null)
                {
                    continue;
                }

                this.idQueue.Enqueue(id);

                fiber.LateUpdate();
            }

            while (this.addIds.Count > 0)
            {
                this.idQueue.Enqueue(this.addIds.Dequeue());
            }
        }

        public void Add(int fiberId = 0)
        {
            this.addIds.Enqueue(fiberId);
        }
    }
}