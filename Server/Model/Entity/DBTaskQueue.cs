using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base;
using Model;

namespace Model
{
	public sealed class DBTaskQueue : Entity
	{
		public Queue<DBTask> queue = new Queue<DBTask>();

		private TaskCompletionSource<DBTask> tcs;

		public async void Start()
		{
			while (true)
			{
				if (this.Id == 0)
				{
					return;
				}
				
				DBTask task = await this.Get();

				try
				{
					await task.Run();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
		public void Add(DBTask task)
		{
			if (this.tcs != null)
			{
				var t = this.tcs;
				this.tcs = null;
				t.SetResult(task);
				return;
			}
			
			this.queue.Enqueue(task);
		}

		public Task<DBTask> Get()
		{
			TaskCompletionSource<DBTask> t = new TaskCompletionSource<DBTask>();
			if (this.queue.Count > 0)
			{
				DBTask task = this.queue.Dequeue();
				t.SetResult(task);
			}
			else
			{
				this.tcs = t;
			}
			return t.Task;
		}
	}
}