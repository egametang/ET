using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
	public abstract class LocationTask : SceneEntity
	{
		protected LocationTask()
		{
		}

		protected LocationTask(long id) : base(id)
		{
		}

		public abstract void Run();
	}

	public sealed class LocationLockTask : LocationTask
	{
		private readonly long key;

		private readonly TaskCompletionSource<bool> tcs;

		public LocationLockTask(long key)
		{
			this.key = key;
			this.tcs = new TaskCompletionSource<bool>();
		}

		public Task<bool> Task
		{
			get
			{
				return this.tcs.Task;
			}
		}

		public override void Run()
		{
			try
			{
				Scene.GetComponent<LocationComponent>().Lock(this.key);
				this.tcs.SetResult(true);
			}
			catch (Exception e)
			{
				this.tcs.SetException(e);
			}
		}
	}

	public sealed class LocationQueryTask : LocationTask
	{
		private readonly long key;

		private readonly TaskCompletionSource<string> tcs;

		public LocationQueryTask(long key)
		{
			this.key = key;
			this.tcs = new TaskCompletionSource<string>();
		}

		public Task<string> Task
		{
			get
			{
				return this.tcs.Task;
			}
		}

		public override void Run()
		{
			try
			{
				string location = Scene.GetComponent<LocationComponent>().Get(key);
				this.tcs.SetResult(location);
			}
			catch (Exception e)
			{
				this.tcs.SetException(e);
			}
		}
	}

	public class LocationComponent : Component
	{
		private readonly Dictionary<long, string> locations = new Dictionary<long, string>();

		private readonly HashSet<long> lockSet = new HashSet<long>();

		private readonly Dictionary<long, Queue<LocationTask>> taskQueues = new Dictionary<long, Queue<LocationTask>>();

		public void Add(long key, string address)
		{
			this.locations[key] = address;
		}

		public void Remove(long key)
		{
			this.locations.Remove(key);
		}

		public string Get(long key)
		{
			this.locations.TryGetValue(key, out string location);
			return location;
		}

		public void Lock(long key)
		{
			if (this.lockSet.Contains(key))
			{
				return;
			}
			this.lockSet.Add(key);
		}

		public void UnLock(long key, string value)
		{
			this.lockSet.Remove(key);

			this.locations[key] = value;

			if (!this.taskQueues.TryGetValue(key, out Queue<LocationTask> tasks))
			{
				return;
			}

			while (true)
			{
				if (tasks.Count <= 0)
				{
					this.taskQueues.Remove(key);
					return;
				}
				if (this.lockSet.Contains(key))
				{
					return;
				}

				LocationTask task = tasks.Dequeue();
				task.Run();
			}
		}

		public Task<bool> LockAsync(long key)
		{
			if (!this.lockSet.Contains(key))
			{
				this.Lock(key);
				return Task.FromResult(true);
			}

			LocationLockTask task = new LocationLockTask(key);
			this.AddTask(key, task);
			return task.Task;
		}

		public Task<string> GetAsync(long key)
		{
			if (!this.lockSet.Contains(key))
			{
				this.locations.TryGetValue(key, out string location);
				return Task.FromResult(location);
			}

			LocationQueryTask task = new LocationQueryTask(key);
			this.AddTask(key, task);
			return task.Task;
		}

		public void AddTask(long key, LocationTask task)
		{
			if (!this.taskQueues.TryGetValue(key, out Queue<LocationTask> tasks))
			{
				tasks = new Queue<LocationTask>();
				this.taskQueues[key] = tasks;
			}
			task.Scene = this.GetOwner<Scene>();
			tasks.Enqueue(task);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();
		}
	}
}