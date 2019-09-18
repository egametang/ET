using System.Collections.Generic;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DbComponentSystem : AwakeSystem<DBComponent>
	{
		public override void Awake(DBComponent self)
		{
			self.Awake();
		}
	}

	/// <summary>
	/// 用来缓存数据
	/// </summary>
	public class DBComponent : Component
	{
		public MongoClient mongoClient;
		public IMongoDatabase database;
		
		public const int taskCount = 32;
		public List<DBTaskQueue> tasks = new List<DBTaskQueue>(taskCount);

		public void Awake()
		{
			DBConfig config = StartConfigComponent.Instance.StartConfig.GetComponent<DBConfig>();
			string connectionString = config.ConnectionString;
			mongoClient = new MongoClient(connectionString);
			this.database = this.mongoClient.GetDatabase(config.DBName);
			
			for (int i = 0; i < taskCount; ++i)
			{
				DBTaskQueue taskQueue = ComponentFactory.Create<DBTaskQueue>();
				this.tasks.Add(taskQueue);
			}
		}
		
		public IMongoCollection<ComponentWithId> GetCollection(string name)
		{
			return this.database.GetCollection<ComponentWithId>(name);
		}

		public ETTask Add(ComponentWithId component, string collectionName = "")
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();

			if (string.IsNullOrEmpty(collectionName))
			{
				collectionName = component.GetType().Name;
			}
			DBSaveTask task = ComponentFactory.CreateWithId<DBSaveTask, ComponentWithId, string, ETTaskCompletionSource>(component.Id, component, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);

			return tcs.Task;
		}

		public ETTask AddBatch(List<ComponentWithId> components, string collectionName)
		{
			ETTaskCompletionSource tcs = new ETTaskCompletionSource();
			DBSaveBatchTask task = ComponentFactory.Create<DBSaveBatchTask, List<ComponentWithId>, string, ETTaskCompletionSource>(components, collectionName, tcs);
			this.tasks[(int)((ulong)task.Id % taskCount)].Add(task);
			return tcs.Task;
		}

		public ETTask<ComponentWithId> Get(string collectionName, long id)
		{
			ETTaskCompletionSource<ComponentWithId> tcs = new ETTaskCompletionSource<ComponentWithId>();
			DBQueryTask dbQueryTask = ComponentFactory.CreateWithId<DBQueryTask, string, ETTaskCompletionSource<ComponentWithId>>(id, collectionName, tcs);
			this.tasks[(int)((ulong)id % taskCount)].Add(dbQueryTask);

			return tcs.Task;
		}

		public ETTask<List<ComponentWithId>> GetBatch(string collectionName, List<long> idList)
		{
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
