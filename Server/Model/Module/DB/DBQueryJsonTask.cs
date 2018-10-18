using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace ETModel
{
	[ObjectSystem]
	public class DBQueryJsonTaskAwakeSystem : AwakeSystem<DBQueryJsonTask, string, string, ETTaskCompletionSource<List<ComponentWithId>>>
	{
		public override void Awake(DBQueryJsonTask self, string collectionName, string json, ETTaskCompletionSource<List<ComponentWithId>> tcs)
		{
			self.CollectionName = collectionName;
			self.Json = json;
			self.Tcs = tcs;
		}
	}

	public sealed class DBQueryJsonTask : DBTask
	{
		public string CollectionName { get; set; }

		public string Json { get; set; }

		public ETTaskCompletionSource<List<ComponentWithId>> Tcs { get; set; }
		
		public override async ETTask Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			try
			{
				// 执行查询数据库任务
				FilterDefinition<ComponentWithId> filterDefinition = new JsonFilterDefinition<ComponentWithId>(this.Json);
				IAsyncCursor<ComponentWithId> cursor = await dbComponent.GetCollection(this.CollectionName).FindAsync(filterDefinition);
				List<ComponentWithId> components = await cursor.ToListAsync();
				this.Tcs.SetResult(components);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"查询数据库异常! {CollectionName} {this.Json}", e));
			}
		}
	}
}