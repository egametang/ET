using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
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

		public async Task Save(ComponentWithId component, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveRequest { Component = component, NeedCache = needCache});
		}

		public async Task SaveBatch(List<ComponentWithId> components, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveBatchRequest { Components = components, NeedCache = needCache});
		}

		public async Task Save(ComponentWithId component, bool needCache, CancellationToken cancellationToken)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveRequest { Component = component, NeedCache = needCache}, cancellationToken);
		}

		public async void SaveLog(ComponentWithId component)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			await session.Call(new DBSaveRequest { Component = component,  NeedCache = false, CollectionName = "Log" });
		}

		public async Task<T> Query<T>(long id, bool needCache = true) where T: ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryResponse dbQueryResponse = (DBQueryResponse)await session.Call(new DBQueryRequest { CollectionName = typeof(T).Name, Id = id, NeedCache = needCache });
			return (T)dbQueryResponse.Component;
		}

		public async Task<List<T>> QueryBatch<T>(List<long> ids, bool needCache = true) where T : ComponentWithId
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryBatchResponse dbQueryBatchResponse = (DBQueryBatchResponse)await session.Call(new DBQueryBatchRequest { CollectionName = typeof(T).Name, IdList = ids, NeedCache = needCache});
			foreach (ComponentWithId component in dbQueryBatchResponse.Components)
			{
				list.Add((T)component);
			}
			return list;
		}

		public async Task<List<T>> QueryJson<T>(string json, bool needCache = true) where T : ComponentWithId
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(dbAddress);
			DBQueryJsonResponse dbQueryJsonResponse = (DBQueryJsonResponse)await session.Call(new DBQueryJsonRequest { CollectionName = typeof(T).Name, Json = json, NeedCache = needCache});
			foreach (ComponentWithId component in dbQueryJsonResponse.Components)
			{
				list.Add((T)component);
			}
			return list;
		}
	}
}