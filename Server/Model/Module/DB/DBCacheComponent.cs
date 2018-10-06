using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
		public Dictionary<string, Dictionary<long, ComponentWithId>> cache = new Dictionary<string, Dictionary<long, ComponentWithId>>();

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

		public Task<bool> Add(ComponentWithId component, string collectionName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			DBSaveTask task = ComponentFactory.CreateWithId<DBSaveTask, ComponentWithId, string, TaskCompletionSource<bool>>(component.Id, component, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public Task<bool> AddBatch(List<ComponentWithId> components, string collectionName)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			DBSaveBatchTask task = ComponentFactory.Create<DBSaveBatchTask, List<ComponentWithId>, string, TaskCompletionSource<bool>>(components, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}

		public void AddToCache(ComponentWithId component, string collectionName = "")
		{
			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			Dictionary<long, ComponentWithId> collection;
			if (!this.cache.TryGetValue(collectionName, out collection))
			{
				collection = new Dictionary<long, ComponentWithId>();
				this.cache.Add(collectionName, collection);
			}
			collection[component.Id] = component;
		}

		public ComponentWithId GetFromCache(string collectionName, long id)
		{
			Dictionary<long, ComponentWithId> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return null;
			}
			ComponentWithId result;
			if (!d.TryGetValue(id, out result))
			{
				return null;
			}
			return result;
		}

		public void RemoveFromCache(string collectionName, long id)
		{
			Dictionary<long, ComponentWithId> d;
			if (!this.cache.TryGetValue(collectionName, out d))
			{
				return;
			}
			d.Remove(id);
		}

		public Task<ComponentWithId> Get(string collectionName, long id)
		{
			ComponentWithId component = GetFromCache(collectionName, id);
			if (component != null)
			{
				return Task.FromResult(component);
			}

			TaskCompletionSource<ComponentWithId> tcs = new TaskCompletionSource<ComponentWithId>();
			DBQueryTask dbQueryTask = ComponentFactory.CreateWithId<DBQueryTask, string, TaskCompletionSource<ComponentWithId>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}

		public Task<List<ComponentWithId>> GetBatch(string collectionName, List<long> idList)
		{
			List <ComponentWithId> components = new List<ComponentWithId>();
			bool isAllInCache = true;
			foreach (long id in idList)
			{
				ComponentWithId component = this.GetFromCache(collectionName, id);
				if (component == null)
				{
					isAllInCache = false;
					break;
				}
				components.Add(component);
			}

			if (isAllInCache)
			{
				return Task.FromResult(components);
			}

			TaskCompletionSource<List<ComponentWithId>> tcs = new TaskCompletionSource<List<ComponentWithId>>();
			DBQueryBatchTask dbQueryBatchTask = ComponentFactory.Create<DBQueryBatchTask, List<long>, string, TaskCompletionSource<List<ComponentWithId>>>(idList, collectionName, tcs);
			this.tasks[(int)((ulong)dbQueryBatchTask.Id % taskCount)].Add(dbQueryBatchTask);

			return tcs.Task;
		}
		
		public Task<List<ComponentWithId>> GetJson(string collectionName, string json)
		{
			TaskCompletionSource<List<ComponentWithId>> tcs = new TaskCompletionSource<List<ComponentWithId>>();
			
			DBQueryJsonTask dbQueryJsonTask = ComponentFactory.Create<DBQueryJsonTask, string, string, TaskCompletionSource<List<ComponentWithId>>>(collectionName, json, tcs);
			this.tasks[(int)((ulong)dbQueryJsonTask.Id % taskCount)].Add(dbQueryJsonTask);

			return tcs.Task;
		}
	}
}
