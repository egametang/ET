using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TNet
{
	public class TPoller: IPoller
	{
		// 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
		private readonly ConcurrentQueue<Action> concurrentQueue = new ConcurrentQueue<Action>();

		private readonly Queue<Action> localQueue = new Queue<Action>();

		public void Add(Action action)
		{
			this.concurrentQueue.Enqueue(action);
		}

		public void Update()
		{
			while (true)
			{
				Action action;
				if (!this.concurrentQueue.TryDequeue(out action))
				{
					break;
				}
				this.localQueue.Enqueue(action);
			}

			while (this.localQueue.Count > 0)
			{
				Action a = this.localQueue.Dequeue();
				a();
			}
		}
	}
}