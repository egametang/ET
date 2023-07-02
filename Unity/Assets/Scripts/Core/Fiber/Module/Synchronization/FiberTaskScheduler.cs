using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
    public class FiberTaskScheduler: TaskScheduler
    {
        private readonly ConcurrentQueue<Task> queue = new();

        public void Update()
        {
            while (true)
            {
                if (!this.queue.TryDequeue(out Task task))
                {
                    return;
                }

                base.TryExecuteTask(task);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this.queue;
        }

        protected override void QueueTask(Task task)
        {
            queue.Enqueue(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            queue.Enqueue(task);
            return true;
        }
    }
}