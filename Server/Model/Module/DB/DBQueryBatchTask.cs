using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Model
{
	[ObjectSystem]
	public class DbQueryBatchTaskSystem : AwakeSystem<DBQueryBatchTask, List<long>, string, TaskCompletionSource<List<Component>>>
	{
		public override void Awake(DBQueryBatchTask self, List<long> idList, string collectionName, TaskCompletionSource<List<Component>> tcs)
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

		public TaskCompletionSource<List<Component>> Tcs { get; set; }
		
		public override async Task Run()
		{
			DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			List<Component> result = new List<Component>();

			try
			{
				// 执行查询数据库任务
				foreach (long id in IdList)
				{
					Component disposer = dbCacheComponent.GetFromCache(this.CollectionName, id);
					if (disposer == null)
					{
						disposer = await dbComponent.GetCollection(this.CollectionName).FindAsync((s) => s.Id == id).Result.FirstOrDefaultAsync();
						dbCacheComponent.AddToCache(disposer);
					}
					
					if (disposer == null)
					{
						continue;
					}
					result.Add(disposer);
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