using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
	[ObjectEvent]
	public class DBCacheComponentEvent : ObjectEvent<DBCacheComponent>, IAwake
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
		public Dictionary<string, Dictionary<long, Entity>> cache = new Dictionary<string, Dictionary<long, Entity>>();

		public const int taskCount = 32;
		public List<DBTaskQueue> tasks = new List<DBTaskQueue>(taskCount);

		public void Awake()
		{
			for (int i = 0; i < taskCount; ++i)
			{
				DBTaskQueue taskQueue = new DBTaskQueue();
				this.tasks.Add(taskQueue);
				taskQueue.Start();
			}
		}

		public Task<bool> Add(Entity entity, string collectionName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			this.AddToCache(entity, collectionName);

			if (collectionName == "")
			{
				collectionName = entity.GetType().Name;
			}
			DBSaveTask task = new DBSaveTask(entity, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public Task<bool> AddBatch(List<Entity> entitys, string collectionName)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			DBSaveBatchTask task = new DBSaveBatchTask(entitys, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}

		public void AddToCache(Entity entity, string collectionName = "")
		{
			if (collectionName == "")
			{
				collectionName = entity.GetType().Name;
			}
			Dictionary<long, Entity> collection;
			if (!this.cache.TryGetValue(collectionName, out collection))
			{
				collection = new Dictionary<long, Entity>();
				this.cache.Add(collectionName, collection);
			}
			collection[entity.Id] = entity;
		}

		public Entity GetFromCache(string collectionName, long id)
		{
			Dictionary<long, Entity> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return null;
			}
			Entity result;
			if (!d.TryGetValue(id, out result))
			{
				return null;
			}
			return result;
		}

		public void RemoveFromCache(string collectionName, long id)
		{
			Dictionary<long, Entity> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return;
			}
			d.Remove(id);
		}

		public Task<Entity> Get(string collectionName, long id)
		{
			Entity entity = GetFromCache(collectionName, id);
			if (entity != null)
			{
				return Task.FromResult(entity);
			}

			TaskCompletionSource<Entity> tcs = new TaskCompletionSource<Entity>();
			this.tasks[(int)((ulong)id % taskCount)].Add(new DBQueryTask(id, collectionName, tcs));

			return tcs.Task;
		}

		public Task<List<Entity>> GetBatch(string collectionName, List<long> idList)
		{
			List <Entity> entitys = new List<Entity>();
			bool isAllInCache = true;
			foreach (long id in idList)
			{
				Entity entity = this.GetFromCache(collectionName, id);
				if (entity == null)
				{
					isAllInCache = false;
					break;
				}
				entitys.Add(entity);
			}

			if (isAllInCache)
			{
				return Task.FromResult(entitys);
			}

			TaskCompletionSource<List<Entity>> tcs = new TaskCompletionSource<List<Entity>>();
			DBQueryBatchTask dbQueryBatchTask = new DBQueryBatchTask(idList, collectionName, tcs);
			this.tasks[(int)((ulong)dbQueryBatchTask.Id % taskCount)].Add(dbQueryBatchTask);

			return tcs.Task;
		}

		public Task<List<Entity>> GetJson(string collectionName, string json)
		{
			TaskCompletionSource<List<Entity>> tcs = new TaskCompletionSource<List<Entity>>();
			
			DBQueryJsonTask dbQueryJsonTask = new DBQueryJsonTask(collectionName, json, tcs);
			this.tasks[(int)((ulong)dbQueryJsonTask.Id % taskCount)].Add(dbQueryJsonTask);

			return tcs.Task;
		}
	}
}