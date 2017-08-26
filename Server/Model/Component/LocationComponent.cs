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

		private readonly int time;

		private readonly TaskCompletionSource<bool> tcs;

		public LocationLockTask(long key, int time)
		{
			this.key = key;
			this.time = time;
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
				Scene.GetComponent<LocationComponent>().Lock(this.key, this.time);
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

		private readonly TaskCompletionSource<int> tcs;

		public LocationQueryTask(long key)
		{
			this.key = key;
			this.tcs = new TaskCompletionSource<int>();
		}

		public Task<int> Task
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
				int location = Scene.GetComponent<LocationComponent>().Get(key);
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
		private readonly Dictionary<long, int> locations = new Dictionary<long, int>();

		private readonly Dictionary<long, int> lockDict = new Dictionary<long, int>();

		private readonly Dictionary<long, Queue<LocationTask>> taskQueues = new Dictionary<long, Queue<LocationTask>>();

		public void Add(long key, int appId)
		{
			this.locations[key] = appId;

			Log.Info($"location add key: {key} appid: {appId}");

			// 更新db
			//await Game.Scene.GetComponent<DBProxyComponent>().Save(new Location(key, address));
		}

		public void Remove(long key)
		{
			Log.Info($"location remove key: {key}");
			this.locations.Remove(key);
		}

		public int Get(long key)
		{
			this.locations.TryGetValue(key, out int location);
			return location;
		}

		public async void Lock(long key, int appId, int time = 0)
		{
			if (this.lockDict.ContainsKey(key))
			{
				return;
			}
			this.lockDict.Add(key, appId);

			// 超时则解锁
			if (time > 0)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(time);

				int saveAppId = 0;
				this.lockDict.TryGetValue(key, out saveAppId);
				if (saveAppId != appId)
				{
					Log.Warning($"unlock appid is different {saveAppId} {appId}");
					return;
				}
				this.UnLock(key);
			}
		}

		public void UpdateAndUnLock(long key, int appId, int value)
		{
			int saveAppId = 0;
			this.lockDict.TryGetValue(key, out saveAppId);
			if (saveAppId != appId)
			{
				Log.Warning($"unlock appid is different {saveAppId} {appId}" );
				return;
			}
			this.Add(key, value);
			this.UnLock(key);
		}

		private void UnLock(long key)
		{
			this.lockDict.Remove(key);

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
				if (this.lockDict.ContainsKey(key))
				{
					return;
				}

				LocationTask task = tasks.Dequeue();
				task.Run();
			}
		}

		public Task<bool> LockAsync(long key, int time)
		{
			if (!this.lockDict.ContainsKey(key))
			{
				this.Lock(key, time);
				return Task.FromResult(true);
			}

			LocationLockTask task = new LocationLockTask(key, time);
			this.AddTask(key, task);
			return task.Task;
		}

		public Task<int> GetAsync(long key)
		{
			if (!this.lockDict.ContainsKey(key))
			{
				Log.Info($"location get key: {key}");
				this.locations.TryGetValue(key, out int location);
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