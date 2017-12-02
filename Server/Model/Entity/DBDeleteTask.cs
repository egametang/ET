using System;
using System.Threading.Tasks;

namespace Model
{
    [ObjectEvent]
    public class DBDeleteTaskEvent : ObjectEvent<DBDeleteTask>, IAwake<string, string, TaskCompletionSource<bool>>
    {
        public void Awake(string filter, string collectionName, TaskCompletionSource<bool> tcs)
        {
            DBDeleteTask task = this.Get();
            task.Filter = filter;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBDeleteTask : DBTask
    {
        public string Filter { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<bool> Tcs { get; set; }

        public DBDeleteTask() { }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                // 执行删除数据库任务
                await dbComponent.GetCollection(this.CollectionName).DeleteOneAsync(Filter);
                this.Tcs.SetResult(true);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"删除数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
