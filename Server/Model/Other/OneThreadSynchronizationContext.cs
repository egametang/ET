using System;
using System.Collections.Generic;
using System.Threading;

namespace Model
{
	public class OneThreadSynchronizationContext : SynchronizationContext
	{
		// 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
		private EQueue<Action> queue = new EQueue<Action>();

		private EQueue<Action> localQueue = new EQueue<Action>();

		private readonly object lockObject = new object();

		private void Add(Action action)
		{
			lock (lockObject)
			{
				this.queue.Enqueue(action);
			}
		}

		public void Update()
		{
			lock (lockObject)
			{
				localQueue = queue;
				queue = new EQueue<Action>();
			}

			while (this.localQueue.Count > 0)
			{
				Action a = this.localQueue.Dequeue();
				a();
			}
		}

		public override void Post(SendOrPostCallback callback, object state)
		{
			this.Add(() => { callback(state); });
		}
	}
}
