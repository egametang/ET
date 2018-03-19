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

		public Task<bool> Add(Component component, string collectionName = "")
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			this.AddToCache(component, collectionName);

			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			DBSaveTask task = ComponentFactory.CreateWithId<DBSaveTask, Component, string, TaskCompletionSource<bool>>(component.Id, component, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public Task<bool> AddBatch(List<Component> components, string collectionName)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			DBSaveBatchTask task = ComponentFactory.Create<DBSaveBatchTask, List<Component>, string, TaskCompletionSource<bool>>(components, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}

		public void AddToCache(Component component, string collectionName = "")
		{
			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			Dictionary<long, Component> collection;
			if (!this.cache.TryGetValue(collectionName, out collection))
			{
				collection = new Dictionary<long, Component>();
				this.cache.Add(collectionName, collection);
			}
			collection[component.Id] = component;
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
			Component component = GetFromCache(collectionName, id);
			if (component != null)
			{
				return Task.FromResult(component);
			}

			TaskCompletionSource<Component> tcs = new TaskCompletionSource<Component>();
			DBQueryTask dbQueryTask = ComponentFactory.CreateWithId<DBQueryTask, string, TaskCompletionSource<Component>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}

		public Task<List<Component>> GetBatch(string collectionName, List<long> idList)
		{
			List <Component> components = new List<Component>();
			bool isAllInCache = true;
			foreach (long id in idList)
			{
				Component component = this.GetFromCache(collectionName, id);
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
