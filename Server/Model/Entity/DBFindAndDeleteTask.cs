using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Model
{
    [ObjectEvent]
    public class DBFindAndDeleteTaskEvent : ObjectEvent<DBFindAndDeleteTask>, IAwake<DBFindAndDeleteOptions, string, TaskCompletionSource<Entity>>
    {
        public void Awake(DBFindAndDeleteOptions options, string collectionName, TaskCompletionSource<Entity> tcs)
        {
            DBFindAndDeleteTask task = this.Get();
            task.TaskFindAndDeleteOptions = options;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBFindAndDeleteTask : DBTask
    {
        public DBFindAndDeleteOptions TaskFindAndDeleteOptions { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<Entity> Tcs { get; set; }

        public DBFindAndDeleteTask() { }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                // 执行查找并删除数据库任务
                var options = new FindOneAndDeleteOptions<Entity, Entity>();
                options.MaxTime = TaskFindAndDeleteOptions.MaxTime;
                options.Projection = TaskFindAndDeleteOptions.Projection;
                options.Sort = TaskFindAndDeleteOptions.Sort;
                Entity result = await dbComponent.GetCollection(this.CollectionName).FindOneAndDeleteAsync(TaskFindAndDeleteOptions.Filter, options);
                this.Tcs.SetResult(result);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"查找并删除数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
