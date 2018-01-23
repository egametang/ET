using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
	[ObjectSystem]
	public class DbTaskQueueSystem : ObjectSystem<DBTaskQueue>, IAwake, IStart
	{
		public void Awake()
		{
			DBTaskQueue self = this.Get();
			self.queue.Clear();
		}

		public async void Start()
		{
			DBTaskQueue self = this.Get();

			while (true)
			{
				if (self.Id == 0)
				{
					return;
				}

				DBTask task = await self.Get();

				try
				{
					await task.Run();

					task.Dispose();
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
	}

	public sealed class DBTaskQueue : Disposer
	{
		public Queue<DBTask> queue = new Queue<DBTask>();

		public TaskCompletionSource<DBTask> tcs;

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
			if (this.queue.Count > 0)
			{
				DBTask task = this.queue.Dequeue();
				return Task.FromResult(task);
			}

			TaskCompletionSource<DBTask> t = new TaskCompletionSource<DBTask>();
			this.tcs = t;
			return t.Task;
		}
	}
}