using System;
using System.Threading;

namespace ET
{

    public class MainThreadSynchronizationContext: Singleton<MainThreadSynchronizationContext>, ISingletonUpdate
    {
        // 创建一个 ThreadSynchronizationContext 类型的字段，用于管理线程同步队列
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new ThreadSynchronizationContext();

        public MainThreadSynchronizationContext()
        {
            // 将当前的同步上下文设置为 threadSynchronizationContext
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }
        
        public void Update()
        {
            // 取出队列中的Action执行
            this.threadSynchronizationContext.Update();
        }

        // 将一个委托和一个状态对象加入到线程同步队列中
        public void Post(SendOrPostCallback callback, object state)
        {
            this.Post(() => callback(state));
        }

        // 将一个委托加入到线程同步队列中
        public void Post(Action action)
        {
            this.threadSynchronizationContext.Post(action);
        }
    }
}