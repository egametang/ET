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

		public ETTask<bool> Add(ComponentWithId component, string collectionName = "")
		{
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();

			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			DBSaveTask task = ComponentFactory.CreateWithId<DBSaveTask, ComponentWithId, string, ETTaskCompletionSource<bool>>(component.Id, component, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public ETTask<bool> AddBatch(List<ComponentWithId> components, string collectionName)
		{
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			DBSaveBatchTask task = ComponentFactory.Create<DBSaveBatchTask, List<ComponentWithId>, string, ETTaskCompletionSource<bool>>(components, collectionName, tcs);
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

		public ETTask<ComponentWithId> Get(string collectionName, long id)
		{
			ComponentWithId component = GetFromCache(collectionName, id);
			if (component != null)
			{
				return ETTask.FromResult(component);
			}

			ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
			DBQueryTask dbQueryTask = ComponentFactory.CreateWithId<DBQueryTask, string, ETTaskCompletionSource<ComponentWithId>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}

		public ETTask<List<ComponentWithId>> GetBatch(string collectionName, List<long> idList)
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
				return ETTask.FromResult(components);
			}

			ETTaskCompletionSource<List<ComponentWithId>> tcs = new ETTaskCompletionSource<List<ComponentWithId>>();
			DBQueryBatchTask dbQueryBatchTask = ComponentFactory.Create<DBQueryBatchTask, List<long>, string, ETTaskCompletionSource<List<ComponentWithId>>>(idList, collectionName, tcs);
			this.tasks[(int)((ulong)dbQueryBatchTask.Id % taskCount)].Add(dbQueryBatchTask);

			return tcs.Task;
		}
		
		public ETTask<List<ComponentWithId>> GetJson(string collectionName, string json)
		{
			ETTaskCompletionSource<List<ComponentWithId>> tcs = new ETTaskCompletionSource<List<ComponentWithId>>();
			
			DBQueryJsonTask dbQueryJsonTask = ComponentFactory.Create<DBQueryJsonTask, string, string, ETTaskCompletionSource<List<ComponentWithId>>>(collectionName, json, tcs);
			this.tasks[(int)((ulong)dbQueryJsonTask.Id % taskCount)].Add(dbQueryJsonTask);

			return tcs.Task;
		}
	}
}
