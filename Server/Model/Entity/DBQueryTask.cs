using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{
	[ObjectEvent]
	public class DBQueryTaskEvent : ObjectEvent<DBQueryTask>, IAwake<string, TaskCompletionSource<Entity>>
	{
		public void Awake(string collectionName, TaskCompletionSource<Entity> tcs)
		{
			DBQueryTask self = this.Get();
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBQueryTask : DBTask
	{
		public string CollectionName { get; set; }

		public TaskCompletionSource<Entity> Tcs { get; set; }

		public DBQueryTask(long id): base(id)
		{
		}

		public override async Task Run()
		{
			DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			// 执行查询前先看看cache中是否已经存在
			Entity entity = dbCacheComponent.GetFromCache(this.CollectionName, this.Id);
			if (entity != null)
			{
				this.Tcs.SetResult(entity);
				return;
			}
			try
			{
				// 执行查询数据库任务
				entity = await dbComponent.GetCollection(this.CollectionName).FindAsync((s) => s.Id == this.Id).Result.FirstOrDefaultAsync();
				if (entity != null)
				{
					dbCacheComponent.AddToCache(entity);
				}
				this.Tcs.SetResult(entity);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {Id}", e));
			}
		}
	}
}