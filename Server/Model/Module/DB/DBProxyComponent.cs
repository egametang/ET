using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Model
{
	[ObjectSystem]
	public class DbProxyComponentSystem : AwakeSystem<DBProxyComponent>
	{
		public override void Awake(DBProxyComponent self)
		{
			self.Awake();
		}
	}
	
	/// <summary>
	/// 用来与数据库操作代理
	/// </summary>
	public class DBProxyComponent : Component
	{
		public IPEndPoint dbAddress;

		public void Awake()
		{
			StartConfig dbStartConfig = Game.Scene.GetComponent<StartConfigComponent>().DBConfig;
			dbAddress = dbStartConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

		public async Task Save(Component disposer, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveRequest { Disposer = disposer, NeedCache = needCache});
		}

		public async Task SaveBatch(List<Component> disposers, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveBatchRequest { Disposers = disposers, NeedCache = needCache});
		}

		public async Task Save(Component disposer, bool needCache, CancellationToken cancellationToken)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveRequest { Disposer = disposer, NeedCache = needCache}, cancellationToken);
		}

		public async void SaveLog(Component disposer)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveRequest { Disposer = disposer,  NeedCache = false, CollectionName = "Log" });
		}

		public async Task<T> Query<T>(long id, bool needCache = true) where T: Component
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryResponse dbQueryResponse = (DBQueryResponse)await session.Call(new DBQueryRequest { CollectionName = typeof(T).Name, Id = id, NeedCache = needCache });
			return (T)dbQueryResponse.Disposer;
		}

		public async Task<List<T>> QueryBatch<T>(List<long> ids, bool needCache = true) where T : Component
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryBatchResponse dbQueryBatchResponse = (DBQueryBatchResponse)await session.Call(new DBQueryBatchRequest { CollectionName = typeof(T).Name, IdList = ids, NeedCache = needCache});
			foreach (Component disposer in dbQueryBatchResponse.Disposers)
			{
				list.Add((T)disposer);
			}
			return list;
		}

		public async Task<List<T>> QueryJson<T>(string json, bool needCache = true) where T : Component
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryJsonResponse dbQueryJsonResponse = (DBQueryJsonResponse)await session.Call(new DBQueryJsonRequest { CollectionName = typeof(T).Name, Json = json, NeedCache = needCache});
			foreach (Component disposer in dbQueryJsonResponse.Disposers)
			{
				list.Add((T)disposer);
			}
			return list;
		}
	}
}