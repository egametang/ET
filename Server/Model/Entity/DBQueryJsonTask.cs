using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{
	[ObjectEvent]
	public class DBQueryJsonTaskEvent : ObjectEvent<DBQueryJsonTask>, IAwake<string, string, TaskCompletionSource<List<Entity>>>
	{
		public void Awake(string collectionName, string json, TaskCompletionSource<List<Entity>> tcs)
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

		public TaskCompletionSource<List<Entity>> Tcs { get; set; }
		
		public override async Task Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			try
			{
				// 执行查询数据库任务
				FilterDefinition<Entity> filterDefinition = new JsonFilterDefinition<Entity>(this.Json);
				List<Entity> entitys = await dbComponent.GetCollection(this.CollectionName).FindAsync(filterDefinition).Result.ToListAsync();
				this.Tcs.SetResult(entitys);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {this.Json}", e));
			}
		}
	}
}