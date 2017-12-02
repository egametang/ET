using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{
    [ObjectEvent]
    public class DBUpdateTaskEvent : ObjectEvent<DBUpdateTask>, IAwake<DBUpdateOptions, string, TaskCompletionSource<bool>>
    {
        public void Awake(DBUpdateOptions options, string collectionName, TaskCompletionSource<bool> tcs)
        {
            DBUpdateTask task = this.Get();
            task.TaskUpdateOptions = options;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBUpdateTask : DBTask
    {
        public DBUpdateOptions TaskUpdateOptions { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<bool> Tcs { get; set; }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                // 执行修改数据库任务
                await dbComponent.GetCollection(this.CollectionName).UpdateOneAsync(TaskUpdateOptions.Filter, TaskUpdateOptions.Update,new UpdateOptions() { IsUpsert = TaskUpdateOptions.IsUpsert });
                this.Tcs.SetResult(true);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"修改数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
