using System;
using System.Collections.Generic;

namespace ET
{
    public class NetServices: Singleton<NetServices>
    {
        // 一个进程最多1024个Service
        private readonly AService[] Services = new AService[1024];

        private int index;
        
        private readonly Queue<int> queue = new Queue<int>();

        public AService Get(int id)
        {
            return this.Services[id];
        }

        public void Add(AService aService)
        {
            for (int j = 0; j < 1024; ++j)
            {
                if (++this.index == 1024)
                {
                    this.index = 0;
                }

                if (this.Services[this.index] != null)
                {
                    continue;
                }

                aService.Id = this.index;
                this.Services[aService.Id] = aService;
                this.queue.Enqueue(aService.Id);
                return;
            }
            throw new Exception("not found service id");
        }

        public void Remove(AService aService)
        {
            this.Services[aService.Id] = null;
        }

        public void Update()
        {
            int count = this.queue.Count;
            while (count-- > 0)
            {
                int serviceId = this.queue.Dequeue();
                AService service = this.Services[serviceId];
                if (service == null)
                {
                    continue;
                }
                this.queue.Enqueue(serviceId);
                service.Update();
            }
        }
    }
}