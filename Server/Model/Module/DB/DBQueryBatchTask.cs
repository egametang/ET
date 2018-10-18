using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DbQueryBatchTaskSystem : AwakeSystem<DBQueryBatchTask, List<long>, string, ETTaskCompletionSource<List<ComponentWithId>>>
	{
		public override void Awake(DBQueryBatchTask self, List<long> idList, string collectionName, ETTaskCompletionSource<List<ComponentWithId>> tcs)
		{
			self.IdList = idList;
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBQueryBatchTask : DBTask
	{
		public string CollectionName { get; set; }

		public List<long> IdList { get; set; }

		public ETTaskCompletionSource<List<ComponentWithId>> Tcs { get; set; }
		
		public override async ETTask Run()
		{
			DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			List<ComponentWithId> result = new List<ComponentWithId>();

			try
			{
				// 执行查询数据库任务
				foreach (long id in IdList)
				{
					ComponentWithId component = dbCacheComponent.GetFromCache(this.CollectionName, id);
					if (component == null)
					{
						IAsyncCursor<ComponentWithId> cursor = await dbComponent.GetCollection(this.CollectionName).FindAsync((s) => s.Id == id);
						component = await cursor.FirstOrDefaultAsync();
					}
					
					if (component == null)
					{
						continue;
					}
					result.Add(component);
				}
				
				this.Tcs.SetResult(result);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {this.CollectionName} {IdList.ListToString()}", e));
			}
		}
	}
}