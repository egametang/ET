using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{

	[ObjectEvent]
	public class DBSaveTaskEvent : ObjectEvent<DBSaveTask>, IAwake<Entity, string, TaskCompletionSource<bool>>
	{
		public void Awake(Entity entity, string collectionName, TaskCompletionSource<bool> tcs)
		{
			DBSaveTask self = this.Get();

			self.Entity = entity;
			self.CollectionName = collectionName;
			self.Tcs = tcs;
		}
	}

	public sealed class DBSaveTask : DBTask
	{
		public Entity Entity;

		public string CollectionName { get; set; }

		public TaskCompletionSource<bool> Tcs;

		public DBSaveTask(long id): base(id)
		{
		}

		public override async Task Run()
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

			try
			{
				// 执行保存数据库任务
				await dbComponent.GetCollection(this.CollectionName).ReplaceOneAsync(s => s.Id == this.Entity.Id, this.Entity, new UpdateOptions {IsUpsert = true});
				this.Tcs.SetResult(true);
			}
			catch (Exception e)
			{
				this.Tcs.SetException(new Exception($"保存数据失败!  {CollectionName} {Id}", e));
			}
		}
	}
}