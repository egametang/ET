using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    [ObjectEvent]
    public class DBInsertBatchTaskEvent : ObjectEvent<DBInsertBatchTask>, IAwake<List<Entity>, string, TaskCompletionSource<bool>>
    {
        public void Awake(List<Entity> insertEntitys, string collectionName, TaskCompletionSource<bool> tcs)
        {
            DBInsertBatchTask task = this.Get();
            task.InsertEntitys = insertEntitys;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBInsertBatchTask : DBTask
    {
        public List<Entity> InsertEntitys { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<bool> Tcs { get; set; }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                // 执行批量插入数据库任务
                await dbComponent.GetCollection(this.CollectionName).InsertManyAsync(InsertEntitys);
                this.Tcs.SetResult(true);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"批量插入数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
