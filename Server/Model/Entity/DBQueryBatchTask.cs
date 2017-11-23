using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Model
{
	[ObjectEvent]
	public class DBQueryBatchTaskEvent : ObjectEvent<DBQueryBatchTask>, IAwake<List<long>, string, TaskCompletionSource<List<Entity>>>
	{
		public void Awake(List<long> idList, string collectionName, TaskCompletionSource<List<Entity>> tcs)
		{
			DBQueryBatchTask self = this.Get();

			self.IdList = idList;
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBQueryBatchTask : DBTask
	{
		public string CollectionName { get; set; }

		public List<long> IdList { get; set; }

		public TaskCompletionSource<List<Entity>> Tcs { get; set; }
		
		public override async Task Run()
		{
			DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			List<Entity> result = new List<Entity>();

			try
			{
				// 执行查询数据库任务
				foreach (long id in IdList)
				{
					Entity entity = dbCacheComponent.GetFromCache(this.CollectionName, id);
					if (entity == null)
					{
						entity = await dbComponent.GetCollection(this.CollectionName).FindAsync((s) => s.Id == id).Result.FirstOrDefaultAsync();
						dbCacheComponent.AddToCache(entity);
					}
					
					if (entity == null)
					{
						continue;
					}
					result.Add(entity);
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