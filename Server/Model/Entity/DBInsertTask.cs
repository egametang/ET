using System;
using System.Threading.Tasks;

namespace Model
{
    [ObjectEvent]
    public class DBInsertTaskEvent : ObjectEvent<DBInsertTask>, IAwake<Entity, string, TaskCompletionSource<bool>>
    {
        public void Awake(Entity insertEntity, string collectionName, TaskCompletionSource<bool> tcs)
        {
            DBInsertTask task = this.Get();
            task.InsertEntity = insertEntity;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBInsertTask : DBTask
    {
        public Entity InsertEntity { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<bool> Tcs { get; set; }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                // 执行插入数据库任务
                await dbComponent.GetCollection(this.CollectionName).InsertOneAsync(InsertEntity);
                this.Tcs.SetResult(true);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"插入数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
