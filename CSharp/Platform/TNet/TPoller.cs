using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TNet
{
	internal class TPoller : IPoller
	{
		// 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行
		private readonly BlockingCollection<Action> blockingCollection = new BlockingCollection<Action>();
		
		public void Add(Action action)
		{
			this.blockingCollection.Add(action);
		}

		public void Dispose()
		{
		}

		public void Run(int timeout)
		{
			// 处理读写线程的回调
			Action action;
			if (this.blockingCollection.TryTake(out action, timeout))
			{

				var queue = new Queue<Action>();
				queue.Enqueue(action);

				while (true)
				{
					if (!this.blockingCollection.TryTake(out action, 0))
					{
						break;
					}
					queue.Enqueue(action);
				}

				while (queue.Count > 0)
				{
					Action a = queue.Dequeue();
					a();
				}
			}
		}
	}
}
