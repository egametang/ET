using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Model
{
	[ObjectEvent]
	public class DBProxyComponentEvent : ObjectEvent<DBProxyComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
	/// <summary>
	/// 用来与数据库操作代理
	/// </summary>
	public class DBProxyComponent : Component
	{
		public string dbAddress;

		public void Awake()
		{
			StartConfig dbStartConfig = Game.Scene.GetComponent<StartConfigComponent>().DBConfig;
			dbAddress = dbStartConfig.GetComponent<InnerConfig>().Address;
		}

		public async Task Save(Disposer disposer, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call<DBSaveResponse>(new DBSaveRequest { Disposer = disposer, NeedCache = needCache});
		}

		public async Task SaveBatch(List<Disposer> disposers, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call<DBSaveBatchResponse>(new DBSaveBatchRequest { Disposers = disposers, NeedCache = needCache});
		}

		public async Task Save(Disposer disposer, bool needCache, CancellationToken cancellationToken)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call<DBSaveResponse>(new DBSaveRequest { Disposer = disposer, NeedCache = needCache}, cancellationToken);
		}

		public async void SaveLog(Disposer disposer)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call<DBSaveResponse>(new DBSaveRequest { Disposer = disposer,  NeedCache = false, CollectionName = "Log" });
		}

		public async Task<T> Query<T>(long id, bool needCache = true) where T: Entity
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryResponse dbQueryResponse = await session.Call<DBQueryResponse>(new DBQueryRequest { CollectionName = typeof(T).Name, Id = id, NeedCache = needCache });
			return (T)dbQueryResponse.Disposer;
		}

		public async Task<List<T>> QueryBatch<T>(List<long> ids, bool needCache = true) where T : Entity
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryBatchResponse dbQueryBatchResponse = await session.Call<DBQueryBatchResponse>(new DBQueryBatchRequest { CollectionName = typeof(T).Name, IdList = ids, NeedCache = needCache});
			foreach (Disposer disposer in dbQueryBatchResponse.Disposers)
			{
				list.Add((T)disposer);
			}
			return list;
		}

		public async Task<List<T>> QueryJson<T>(string json, bool needCache = true) where T : Entity
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryJsonResponse dbQueryJsonResponse = await session.Call<DBQueryJsonResponse>(new DBQueryJsonRequest { CollectionName = typeof(T).Name, Json = json, NeedCache = needCache});
			foreach (Disposer disposer in dbQueryJsonResponse.Disposers)
			{
				list.Add((T)disposer);
			}
			return list;
		}
	}
}