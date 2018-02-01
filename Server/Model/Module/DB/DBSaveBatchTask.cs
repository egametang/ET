using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Model
{
	[ObjectSystem]
	public class DbSaveBatchTaskSystem : ObjectSystem<DBSaveBatchTask>, IAwake<List<Disposer>, string, TaskCompletionSource<bool>>
	{
		public void Awake(List<Disposer> disposers, string collectionName, TaskCompletionSource<bool> tcs)
		{
			DBSaveBatchTask self = this.Get();
			
			self.Disposers = disposers;
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBSaveBatchTask : DBTask
	{
		public string CollectionName { get; set; }

		public List<Disposer> Disposers;

		public TaskCompletionSource<bool> Tcs;
	
		public override async Task Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

			foreach (Disposer disposer in this.Disposers)
			{
				if (disposer == null)
				{
					continue;
				}

				try
				{
					// 执行保存数据库任务
					await dbComponent.GetCollection(this.CollectionName).ReplaceOneAsync(s => s.Id == disposer.Id, disposer, new UpdateOptions { IsUpsert = true });
				}
				catch (Exception e)
				{
					Log.Debug($"{disposer.GetType().Name} {disposer.ToJson()} {e}");
					this.Tcs.SetException(new Exception($"保存数据失败! {CollectionName} {this.Disposers.ListToString()}", e));
				}
			}
			this.Tcs.SetResult(true);
		}
	}
}