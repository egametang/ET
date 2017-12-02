using System;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Model
{
    [ObjectEvent]
    public class DBFindBatchTaskEvent : ObjectEvent<DBFindBatchTask>, IAwake<DBFindOptions, string, TaskCompletionSource<List<Entity>>>
    {
        public void Awake(DBFindOptions options, string collectionName, TaskCompletionSource<List<Entity>> tcs)
        {
            DBFindBatchTask task = this.Get();
            task.TaskFindOptions = options;
            task.CollectionName = collectionName;
            task.Tcs = tcs;
        }
    }

    public sealed class DBFindBatchTask : DBTask
    {
        public DBFindOptions TaskFindOptions { get; set; }

        public string CollectionName { get; set; }

        public TaskCompletionSource<List<Entity>> Tcs { get; set; }

        public DBFindBatchTask() { }

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

                // 执行批量查询数据库任务
                List<Entity> result = await dbComponent.GetCollection(this.CollectionName).FindAsync(TaskFindOptions.Filter, findOptions).Result.ToListAsync();
                this.Tcs.SetResult(result);
            }
            catch (Exception e)
            {
                this.Tcs.SetException(new Exception($"批量查询数据失败!  {CollectionName} {Id}", e));
            }
        }
    }
}
