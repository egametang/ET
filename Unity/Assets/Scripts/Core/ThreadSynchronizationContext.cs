using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ET
{
    public class ThreadSynchronizationContext : SynchronizationContext
    {
        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行，避免了多线程的竞争和冲突。
        private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

        private Action a;

        public void Update()
        {
            while (true)
            {
                // 取出动作
                if (!this.queue.TryDequeue(out a))
                {
                    return;
                }

                try
                {
                    // 执行动作
                    a();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        // 加入队列
        public override void Post(SendOrPostCallback callback, object state)
        {
            this.Post(() => callback(state));
        }

        // 加入队列
        public void Post(Action action)
        {
            this.queue.Enqueue(action);
        }
    }
}