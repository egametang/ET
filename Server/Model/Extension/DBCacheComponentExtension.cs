using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public static class DBCacheComponentExtension
    {
        /// <summary>
        /// 插入一个
        /// </summary>
        /// <param name="self"></param>
        /// <param name="entity">插入对象</param>
        /// <param name="collectionName"插入集合名></param>
        /// <returns></returns>
        public static Task<bool> Insert(this DBCacheComponent self, Entity entity, string collectionName = "")
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (collectionName == "")
            {
                collectionName = entity.GetType().Name;
            }
            DBInsertTask task = EntityFactory.CreateWithId<DBInsertTask, Entity, string, TaskCompletionSource<bool>>(entity.Id, entity, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="self"></param>
        /// <param name="entitys">插入对象</param>
        /// <param name="collectionName">插入集合名</param>
        /// <returns></returns>
        public static Task<bool> InsertBatch(this DBCacheComponent self, List<Entity> entitys, string collectionName = "")
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (collectionName == "")
            {
                collectionName = entitys[0].GetType().Name;
            }
            DBInsertBatchTask task = EntityFactory.Create<DBInsertBatchTask, List<Entity>, string, TaskCompletionSource<bool>>(entitys, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 删除一个
        /// </summary>
        /// <param name="self"></param>
        /// <param name="filter">过滤器</param>
        /// <param name="collectionName">删除集合名</param>
        /// <returns></returns>
        public static Task<bool> Delete(this DBCacheComponent self, string filter, string collectionName)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (string.IsNullOrEmpty(collectionName))
            {
                tcs.SetException(new Exception($"删除数据失败，CollectionName为空!  {collectionName} {filter}"));
            }

            DBDeleteTask task = EntityFactory.Create<DBDeleteTask, string, string, TaskCompletionSource<bool>>(filter, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="filter">过滤器</param>
        /// <param name="collectionName">删除集合名</param>
        /// <returns></returns>
        public static Task<bool> DeleteBatch(this DBCacheComponent self, string filter, string collectionName)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (string.IsNullOrEmpty(collectionName))
            {
                tcs.SetException(new Exception($"批量删除数据失败，CollectionName为空!  {collectionName} {filter}"));
            }

            DBDeleteBatchTask task = EntityFactory.Create<DBDeleteBatchTask, string, string, TaskCompletionSource<bool>>(filter, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 修改一个
        /// </summary>
        /// <param name="self"></param>
        /// <param name="options">修改选项</param>
        /// <param name="collectionName">修改集合名</param>
        /// <returns></returns>
        public static Task<bool> Update(this DBCacheComponent self, DBUpdateOptions options, string collectionName)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (string.IsNullOrEmpty(collectionName))
            {
                tcs.SetException(new Exception($"修改数据失败，CollectionName为空!  {collectionName}"));
            }

            DBUpdateTask task = EntityFactory.Create<DBUpdateTask, DBUpdateOptions, string, TaskCompletionSource<bool>>(options, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="self"></param>
        /// <param name="options">修改选项</param>
        /// <param name="collectionName">修改集合名</param>
        /// <returns></returns>
        public static Task<bool> UpdateBatch(this DBCacheComponent self, DBUpdateOptions options, string collectionName)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (string.IsNullOrEmpty(collectionName))
            {
                tcs.SetException(new Exception($"批量修改数据失败，CollectionName为空!  {collectionName}"));
            }

            DBUpdateBatchTask task = EntityFactory.Create<DBUpdateBatchTask, DBUpdateOptions, string, TaskCompletionSource<bool>>(options, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 查询一个
        /// </summary>
        /// <param name="self"></param>
        /// <param name="options">查询选项</param>
        /// <param name="collectionName">查询集合名</param>
        /// <returns></returns>
        public static Task<Entity> Find(this DBCacheComponent self, DBFindOptions options, string collectionName)
        {
            TaskCompletionSource<Entity> tcs = new TaskCompletionSource<Entity>();

            if (string.IsNullOrEmpty(collectionName))
            {
                tcs.SetException(new Exception($"查询数据失败，CollectionName为空!  {collectionName}"));
            }

            DBFindTask task = EntityFactory.Create<DBFindTask, DBFindOptions, string, TaskCompletionSource<Entity>>(options, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 批量查询
        /// </summary>
        /// <param name="self"></param>
        /// <param name="options">查询选项</param>
        /// <param name="collectionName">查询集合名</param>
        /// <returns></returns>
        public static Task<List<Entity>> FindBatch(this DBCacheComponent self, DBFindOptions options, string collectionName)
        {
            TaskCompletionSource<List<Entity>> tcs = new TaskCompletionSource<List<Entity>>();

            if (string.IsNullOrEmpty(collectionName))
            {
                tcs.SetException(new Exception($"批量查询数据失败，CollectionName为空!  {collectionName}"));
            }

            DBFindBatchTask task = EntityFactory.Create<DBFindBatchTask, DBFindOptions, string, TaskCompletionSource<List<Entity>>>(options, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }

        /// <summary>
        /// 查询并删除数据
        /// </summary>
        /// <param name="self"></param>
        /// <param name="options">查询并删除选项</param>
        /// <param name="collectionName">查询并删除集合名</param>
        /// <returns></returns>
        public static Task<Entity> FindAndDelete(this DBCacheComponent self, DBFindAndDeleteOptions options, string collectionName)
        {
            TaskCompletionSource<Entity> tcs = new TaskCompletionSource<Entity>();

            if (string.IsNullOrEmpty(collectionName))
            {
                tcs.SetException(new Exception($"查询并删除数据失败，CollectionName为空!  {collectionName}"));
            }

            DBFindAndDeleteTask task = EntityFactory.Create<DBFindAndDeleteTask, DBFindAndDeleteOptions, string, TaskCompletionSource<Entity>>(options, collectionName, tcs);
            self.tasks[(int)((ulong)task.Id % DBCacheComponent.taskCount)].Add(task);

            return tcs.Task;
        }
    }
}
