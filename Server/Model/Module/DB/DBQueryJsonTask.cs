using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{
	[ObjectSystem]
	public class DbQueryJsonTaskSystem : ObjectSystem<DBQueryJsonTask>, IAwake<string, string, TaskCompletionSource<List<Disposer>>>
	{
		public void Awake(string collectionName, string json, TaskCompletionSource<List<Disposer>> tcs)
		{
			DBQueryJsonTask self = this.Get();
			
			self.CollectionName = collectionName;
			self.Json = json;
			self.Tcs = tcs;
		}
	}

	public sealed class DBQueryJsonTask : DBTask
	{
		public string CollectionName { get; set; }

		public string Json { get; set; }

		public TaskCompletionSource<List<Disposer>> Tcs { get; set; }
		
		public override async Task Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			try
			{
				// 执行查询数据库任务
				FilterDefinition<Disposer> filterDefinition = new JsonFilterDefinition<Disposer>(this.Json);
				List<Disposer> disposers = await dbComponent.GetCollection(this.CollectionName).FindAsync(filterDefinition).Result.ToListAsync();
				this.Tcs.SetResult(disposers);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {this.Json}", e));
			}
		}
	}
}