using System;
using System.Threading.Tasks;

namespace Model
{
    [ObjectEvent]
    public class DBDeleteBatchTaskEvent : ObjectEvent<DBDeleteBatchTask>, IAwake<string, string, TaskCompletionSource<bool>>
    {
        public void Awake(string filter, string collectionName, TaskCompletionSource<bool> tcs)
        {
            DBDeleteBatchTask task = this.Get();
            task.Filter = filter;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBDeleteBatchTask : DBTask
    {
        public string Filter { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<bool> Tcs { get; set; }

        public DBDeleteBatchTask() { }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                // 执行批量删除数据库任务
                await dbComponent.GetCollection(this.CollectionName).DeleteManyAsync(Filter);
                this.Tcs.SetResult(true);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"批量删除数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
