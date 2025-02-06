using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ET
{
    public class ThreadSynchronizationContext : SynchronizationContext
    {
        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
        private readonly ConcurrentQueue<Action> queue = new();

        private Action a;

        public void Update()
        {
            while (true)
            {
                if (!this.queue.TryDequeue(out a))
                {
                    return;
                }

                try
                {
                    a();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            this.Post(() => callback(state));
        }
		
        public void Post(Action action)
        {
            this.queue.Enqueue(action);
        }
    }
}