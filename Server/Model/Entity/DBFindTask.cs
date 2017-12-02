using System;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Model
{
    [ObjectEvent]
    public class DBFindTaskEvent : ObjectEvent<DBFindTask>, IAwake<DBFindOptions, string, TaskCompletionSource<Entity>>
    {
        public void Awake(DBFindOptions options, string collectionName, TaskCompletionSource<Entity> tcs)
        {
            DBFindTask task = this.Get();
            task.TaskFindOptions = options;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBFindTask : DBTask
    {
        public DBFindOptions TaskFindOptions { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<Entity> Tcs { get; set; }

        public override async Task Run()
        {
            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

            try
            {
                var findOptions = new FindOptions<Entity, Entity>();
                findOptions.Limit = TaskFindOptions.Limit;
                findOptions.Skip = TaskFindOptions.Skip;
                findOptions.Sort = TaskFindOptions.Sort;
                findOptions.Projection = TaskFindOptions.Projection;

                // 执行查询数据库任务
                Entity result = await dbComponent.GetCollection(this.CollectionName).FindAsync(TaskFindOptions.Filter, findOptions).Result.FirstOrDefaultAsync();
                this.Tcs.SetResult(result);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"查询数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
