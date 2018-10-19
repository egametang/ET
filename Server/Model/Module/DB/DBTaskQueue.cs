using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class DbTaskQueueAwakeSystem : AwakeSystem<DBTaskQueue>
	{
		public override void Awake(DBTaskQueue self)
		{
			self.queue.Clear();
		}
	}

	[ObjectSystem]
	public class DbTaskQueueStartSystem : StartSystem<DBTaskQueue>
	{
		public override void Start(DBTaskQueue self)
		{
			StartAsync(self).NoAwait();
		}
		
		public async ETVoid StartAsync(DBTaskQueue self)
		{
			long instanceId = self.InstanceId;
			
			while (true)
			{
				if (self.InstanceId != instanceId)
				{
					return;
				}

				DBTask task = await self.Get();

				try
				{
					await task.Run();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}

				task.Dispose();
			}
		}
	}

	public sealed class DBTaskQueue : Component
	{
		public Queue<DBTask> queue = new Queue<DBTask>();

		public ETTaskCompletionSource<DBTask> tcs;

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

		public ETTask<DBTask> Get()
		{
			if (this.queue.Count > 0)
			{
				DBTask task = this.queue.Dequeue();
				return ETTask.FromResult(task);
			}

			ETTaskCompletionSource<DBTask> t = new ETTaskCompletionSource<DBTask>();
			this.tcs = t;
			return t.Task;
		}
	}
}