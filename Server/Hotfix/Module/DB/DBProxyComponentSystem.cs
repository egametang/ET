using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
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
	public static class DBProxyComponentEx
	{
		public static void Awake(this DBProxyComponent self)
		{
			StartConfig dbStartConfig = Game.Scene.GetComponent<StartConfigComponent>().DBConfig;
			self.dbAddress = dbStartConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

		public static async Task Save(this DBProxyComponent self, ComponentWithId component, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component, NeedCache = needCache});
		}

		public static async Task SaveBatch(this DBProxyComponent self, List<ComponentWithId> components, bool needCache = true)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveBatchRequest { Components = components, NeedCache = needCache});
		}

		public static async Task Save(this DBProxyComponent self, ComponentWithId component, bool needCache, CancellationToken cancellationToken)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component, NeedCache = needCache}, cancellationToken);
		}

		public static async void SaveLog(this DBProxyComponent self, ComponentWithId component)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			await session.Call(new DBSaveRequest { Component = component,  NeedCache = false, CollectionName = "Log" });
		}

		public static async Task<T> Query<T>(this DBProxyComponent self, long id, bool needCache = true) where T: ComponentWithId
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryResponse dbQueryResponse = (DBQueryResponse)await session.Call(new DBQueryRequest { CollectionName = typeof(T).Name, Id = id, NeedCache = needCache });
			return (T)dbQueryResponse.Component;
		}

		public static async Task<List<T>> QueryBatch<T>(this DBProxyComponent self, List<long> ids, bool needCache = true) where T : ComponentWithId
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryBatchResponse dbQueryBatchResponse = (DBQueryBatchResponse)await session.Call(new DBQueryBatchRequest { CollectionName = typeof(T).Name, IdList = ids, NeedCache = needCache});
			foreach (ComponentWithId component in dbQueryBatchResponse.Components)
			{
				list.Add((T)component);
			}
			return list;
		}

		public static async Task<List<T>> QueryJson<T>(this DBProxyComponent self, string json) where T : ComponentWithId
		{
			List<T> list = new List<T>();
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
			DBQueryJsonResponse dbQueryJsonResponse = (DBQueryJsonResponse)await session.Call(new DBQueryJsonRequest { CollectionName = typeof(T).Name, Json = json });
			foreach (ComponentWithId component in dbQueryJsonResponse.Components)
			{
				list.Add((T)component);
			}
			return list;
		}
	}
}