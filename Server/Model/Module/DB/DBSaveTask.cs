using System;
using MongoDB.Driver;

namespace ETModel
{

	[ObjectSystem]
	public class DbSaveTaskAwakeSystem : AwakeSystem<DBSaveTask, ComponentWithId, string, ETTaskCompletionSource>
	{
		public override void Awake(DBSaveTask self, ComponentWithId component, string collectionName, ETTaskCompletionSource tcs)
		{
			self.Component = component;
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBSaveTask : DBTask
	{
		public ComponentWithId Component;

		public string CollectionName { get; set; }

		public ETTaskCompletionSource Tcs;

		public override async ETTask Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

			try
			{
				// 执行保存数据库任务
				await dbComponent.GetCollection(this.CollectionName).ReplaceOneAsync(s => s.Id == this.Component.Id, this.Component, new UpdateOptions {IsUpsert = true});
				this.Tcs.SetResult();
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"保存数据失败!  {CollectionName} {Id}", e));
			}
		}
	}
}