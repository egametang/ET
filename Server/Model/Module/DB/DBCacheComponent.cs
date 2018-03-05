using System.Collections.Generic;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class DbCacheComponentSystem : AwakeSystem<DBCacheComponent>
	{
		public override void Awake(DBCacheComponent self)
		{
			self.Awake();
		}
	}

	/// <summary>
	/// 用来缓存数据
	/// </summary>
	public class DBCacheComponent : Component
	{
		public Dictionary<string, Dictionary<long, Component>> cache = new Dictionary<string, Dictionary<long, Component>>();

		public const int taskCount = 32;
		public List<DBTaskQueue> tasks = new List<DBTaskQueue>(taskCount);

		public void Awake()
		{
			for (int i = 0; i < taskCount; ++i)
			{
				DBTaskQueue taskQueue = ComponentFactory.Create<DBTaskQueue>();
				this.tasks.Add(taskQueue);
			}
		}

		public Task<bool> Add(Component disposer, string collectionName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			this.AddToCache(disposer, collectionName);

			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = disposer.GetType().Name;
			}
			DBSaveTask task = ComponentFactory.CreateWithId<DBSaveTask, Component, string, TaskCompletionSource<bool>>(disposer.Id, disposer, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public Task<bool> AddBatch(List<Component> disposers, string collectionName)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			DBSaveBatchTask task = ComponentFactory.Create<DBSaveBatchTask, List<Component>, string, TaskCompletionSource<bool>>(disposers, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}

		public void AddToCache(Component disposer, string collectionName = "")
		{
			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = disposer.GetType().Name;
			}
			Dictionary<long, Component> collection;
			if (!this.cache.TryGetValue(collectionName, out collection))
			{
				collection = new Dictionary<long, Component>();
				this.cache.Add(collectionName, collection);
			}
			collection[disposer.Id] = disposer;
		}

		public Component GetFromCache(string collectionName, long id)
		{
			Dictionary<long, Component> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return null;
			}
			Component result;
			if (!d.TryGetValue(id, out result))
			{
				return null;
			}
			return result;
		}

		public void RemoveFromCache(string collectionName, long id)
		{
			Dictionary<long, Component> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return;
			}
			d.Remove(id);
		}

		public Task<Component> Get(string collectionName, long id)
		{
			Component disposer = GetFromCache(collectionName, id);
			if (disposer != null)
			{
				return Task.FromResult(disposer);
			}

			TaskCompletionSource<Component> tcs = new TaskCompletionSource<Component>();
			DBQueryTask dbQueryTask = ComponentFactory.CreateWithId<DBQueryTask, string, TaskCompletionSource<Component>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}

		public Task<List<Component>> GetBatch(string collectionName, List<long> idList)
		{
			List <Component> disposers = new List<Component>();
			bool isAllInCache = true;
			foreach (long id in idList)
			{
				Component disposer = this.GetFromCache(collectionName, id);
				if (disposer == null)
				{
					isAllInCache = false;
					break;
				}
				disposers.Add(disposer);
			}

			if (isAllInCache)
			{
				return Task.FromResult(disposers);
			}

			TaskCompletionSource<List<Component>> tcs = new TaskCompletionSource<List<Component>>();
			DBQueryBatchTask dbQueryBatchTask = ComponentFactory.Create<DBQueryBatchTask, List<long>, string, TaskCompletionSource<List<Component>>>(idList, collectionName, tcs);
			this.tasks[(int)((ulong)dbQueryBatchTask.Id % taskCount)].Add(dbQueryBatchTask);

			return tcs.Task;
		}

		public Task<List<Component>> GetJson(string collectionName, string json)
		{
			TaskCompletionSource<List<Component>> tcs = new TaskCompletionSource<List<Component>>();
			
			DBQueryJsonTask dbQueryJsonTask = ComponentFactory.Create<DBQueryJsonTask, string, string, TaskCompletionSource<List<Component>>>(collectionName, json, tcs);
			this.tasks[(int)((ulong)dbQueryJsonTask.Id % taskCount)].Add(dbQueryJsonTask);

			return tcs.Task;
		}
	}
}
