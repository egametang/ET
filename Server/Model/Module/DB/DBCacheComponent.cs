using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
	[ObjectSystem]
	public class DbCacheComponentSystem : ObjectSystem<DBCacheComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	/// <summary>
	/// 用来缓存数据
	/// </summary>
	public class DBCacheComponent : Component
	{
		public Dictionary<string, Dictionary<long, Disposer>> cache = new Dictionary<string, Dictionary<long, Disposer>>();

		public const int taskCount = 32;
		public List<DBTaskQueue> tasks = new List<DBTaskQueue>(taskCount);

		public void Awake()
		{
			for (int i = 0; i < taskCount; ++i)
			{
				DBTaskQueue taskQueue = EntityFactory.Create<DBTaskQueue>();
				this.tasks.Add(taskQueue);
			}
		}

		public Task<bool> Add(Disposer disposer, string collectionName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			this.AddToCache(disposer, collectionName);

			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = disposer.GetType().Name;
			}
			DBSaveTask task = EntityFactory.CreateWithId<DBSaveTask, Disposer, string, TaskCompletionSource<bool>>(disposer.Id, disposer, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public Task<bool> AddBatch(List<Disposer> disposers, string collectionName)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			DBSaveBatchTask task = EntityFactory.Create<DBSaveBatchTask, List<Disposer>, string, TaskCompletionSource<bool>>(disposers, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}

		public void AddToCache(Disposer disposer, string collectionName = "")
		{
			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = disposer.GetType().Name;
			}
			Dictionary<long, Disposer> collection;
			if (!this.cache.TryGetValue(collectionName, out collection))
			{
				collection = new Dictionary<long, Disposer>();
				this.cache.Add(collectionName, collection);
			}
			collection[disposer.Id] = disposer;
		}

		public Disposer GetFromCache(string collectionName, long id)
		{
			Dictionary<long, Disposer> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return null;
			}
			Disposer result;
			if (!d.TryGetValue(id, out result))
			{
				return null;
			}
			return result;
		}

		public void RemoveFromCache(string collectionName, long id)
		{
			Dictionary<long, Disposer> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return;
			}
			d.Remove(id);
		}

		public Task<Disposer> Get(string collectionName, long id)
		{
			Disposer disposer = GetFromCache(collectionName, id);
			if (disposer != null)
			{
				return Task.FromResult(disposer);
			}

			TaskCompletionSource<Disposer> tcs = new TaskCompletionSource<Disposer>();
			DBQueryTask dbQueryTask = EntityFactory.CreateWithId<DBQueryTask, string, TaskCompletionSource<Disposer>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}

		public Task<List<Disposer>> GetBatch(string collectionName, List<long> idList)
		{
			List <Disposer> disposers = new List<Disposer>();
			bool isAllInCache = true;
			foreach (long id in idList)
			{
				Disposer disposer = this.GetFromCache(collectionName, id);
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

			TaskCompletionSource<List<Disposer>> tcs = new TaskCompletionSource<List<Disposer>>();
			DBQueryBatchTask dbQueryBatchTask = EntityFactory.Create<DBQueryBatchTask, List<long>, string, TaskCompletionSource<List<Disposer>>>(idList, collectionName, tcs);
			this.tasks[(int)((ulong)dbQueryBatchTask.Id % taskCount)].Add(dbQueryBatchTask);

			return tcs.Task;
		}

		public Task<List<Disposer>> GetJson(string collectionName, string json)
		{
			TaskCompletionSource<List<Disposer>> tcs = new TaskCompletionSource<List<Disposer>>();
			
			DBQueryJsonTask dbQueryJsonTask = EntityFactory.Create<DBQueryJsonTask, string, string, TaskCompletionSource<List<Disposer>>>(collectionName, json, tcs);
			this.tasks[(int)((ulong)dbQueryJsonTask.Id % taskCount)].Add(dbQueryJsonTask);

			return tcs.Task;
		}
	}
}
