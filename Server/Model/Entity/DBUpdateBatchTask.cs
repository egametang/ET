using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ObjectEvent]
    public class DBUpdateBatchTaskEvent : ObjectEvent<DBUpdateBatchTask>, IAwake<DBUpdateOptions, string, TaskCompletionSource<bool>>
    {
        public void Awake(DBUpdateOptions options, string collectionName, TaskCompletionSource<bool> tcs)
        {
            DBUpdateBatchTask task = this.Get();
            task.TaskUpdateOptions = options;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBUpdateBatchTask : DBTask
    {
        public DBUpdateOptions TaskUpdateOptions { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<bool> Tcs { get; set; }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                // 执行批量修改数据库任务
                await dbComponent.GetCollection(this.CollectionName).UpdateManyAsync(TaskUpdateOptions.Filter, TaskUpdateOptions.Update, new MongoDB.Driver.UpdateOptions() { IsUpsert = TaskUpdateOptions.IsUpsert });
                this.Tcs.SetResult(true);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"批量修改数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
