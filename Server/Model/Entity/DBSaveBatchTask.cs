using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Model
{
	[ObjectEvent]
	public class DbSaveBatchTaskSystem : ObjectSystem<DBSaveBatchTask>, IAwake<List<Entity>, string, TaskCompletionSource<bool>>
	{
		public void Awake(List<Entity> entitys, string collectionName, TaskCompletionSource<bool> tcs)
		{
			DBSaveBatchTask self = this.Get();
			
			self.Entitys = entitys;
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBSaveBatchTask : DBTask
	{
		public string CollectionName { get; set; }

		public List<Entity> Entitys;

		public TaskCompletionSource<bool> Tcs;
	
		public override async Task Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

			foreach (Entity entity in this.Entitys)
			{
				if (entity == null)
				{
					continue;
				}

				try
				{
					// 执行保存数据库任务
					await dbComponent.GetCollection(this.CollectionName).ReplaceOneAsync(s => s.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
				}
				catch (Exception e)
				{
					Log.Debug($"{entity.GetType().Name} {entity.ToJson()}" + e.ToString());
					this.Tcs.SetException(new Exception($"保存数据失败! {CollectionName} {this.Entitys.ListToString()}", e));
				}
			}
			this.Tcs.SetResult(true);
		}
	}
}