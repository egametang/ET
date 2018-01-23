using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{
	[ObjectSystem]
	public class DbQueryTaskSystem : ObjectSystem<DBQueryTask>, IAwake<string, TaskCompletionSource<Disposer>>
	{
		public void Awake(string collectionName, TaskCompletionSource<Disposer> tcs)
		{
			DBQueryTask self = this.Get();
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBQueryTask : DBTask
	{
		public string CollectionName { get; set; }

		public TaskCompletionSource<Disposer> Tcs { get; set; }

		public override async Task Run()
		{
			DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			// 执行查询前先看看cache中是否已经存在
			Disposer disposer = dbCacheComponent.GetFromCache(this.CollectionName, this.Id);
			if (disposer != null)
			{
				this.Tcs.SetResult(disposer);
				return;
			}
			try
			{
				// 执行查询数据库任务
				disposer = await dbComponent.GetCollection(this.CollectionName).FindAsync((s) => s.Id == this.Id).Result.FirstOrDefaultAsync();
				if (disposer != null)
				{
					dbCacheComponent.AddToCache(disposer);
				}
				this.Tcs.SetResult(disposer);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {Id}", e));
			}
		}
	}
}