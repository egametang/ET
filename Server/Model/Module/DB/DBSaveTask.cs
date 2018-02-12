using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{

	[ObjectSystem]
	public class DbSaveTaskAwakeSystem : AwakeSystem<DBSaveTask, Component, string, TaskCompletionSource<bool>>
	{
		public override void Awake(DBSaveTask self, Component entity, string collectionName, TaskCompletionSource<bool> tcs)
		{
			self.Disposer = entity;
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBSaveTask : DBTask
	{
		public Component Disposer;

		public string CollectionName { get; set; }

		public TaskCompletionSource<bool> Tcs;

		public override async Task Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

			try
			{
				// 执行保存数据库任务
				await dbComponent.GetCollection(this.CollectionName).ReplaceOneAsync(s => s.Id == this.Disposer.Id, this.Disposer, new UpdateOptions {IsUpsert = true});
				this.Tcs.SetResult(true);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"保存数据失败!  {CollectionName} {Id}", e));
			}
		}
	}
}