using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver;

namespace ET
{
	public class DBComponentAwakeSystem : AwakeSystem<DBComponent, string, string>
	{
		public override void Awake(DBComponent self, string dbConnection, string dbName)
		{
			self.mongoClient = new MongoClient(dbConnection);
			self.database = self.mongoClient.GetDatabase(dbName);
			
			self.Transfers.Clear();
			foreach (Type type in Game.EventSystem.GetTypes())
			{
				if (type == typeof (IDBCollection))
				{
					continue;
				}
				if (!typeof(IDBCollection).IsAssignableFrom(type))
				{
					continue;
				}
				self.Transfers.Add(type.Name);
			}
			
			DBComponent.Instance = self;
		}
	}
	
    public class DBComponentDestroySystem: DestroySystem<DBComponent>
    {
        public override void Destroy(DBComponent self)
        {
	        DBComponent.Instance = null;
	        self.Transfers.Clear();
        }
    }
	
    public static class DBComponentSystem
    {
	    #region Query

	    public static async ETTask<T> Query<T>(this DBComponent self, long id, string collection = null) where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, id % DBComponent.TaskCount))
		    {
			    IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(d => d.Id == id);

			    return await cursor.FirstOrDefaultAsync();
		    }
	    }
	    
	    public static async ETTask<List<T>> Query<T>(this DBComponent self, Expression<Func<T, bool>> filter, string collection = null)
			    where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
		    {
			    IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filter);

			    return await cursor.ToListAsync();
		    }
	    }

	    public static async ETTask<List<T>> Query<T>(this DBComponent self, long taskId, Expression<Func<T, bool>> filter, string collection = null)
			    where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, taskId % DBComponent.TaskCount))
		    {
			    IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filter);

			    return await cursor.ToListAsync();
		    }
	    }
	    
	    public static async ETTask Query(this DBComponent self, long id, List<string> collectionNames, List<Entity> result)
	    {
		    if (collectionNames == null || collectionNames.Count == 0)
		    {
			    return;
		    }

		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, id % DBComponent.TaskCount))
		    {
			    foreach (string collectionName in collectionNames)
			    {
				    IAsyncCursor<Entity> cursor = await self.GetCollection(collectionName).FindAsync(d => d.Id == id);

				    Entity e = await cursor.FirstOrDefaultAsync();

				    if (e == null)
				    {
					    continue;
				    }

				    result.Add(e);
			    }
		    }
	    }

	    public static async ETTask<List<T>> QueryJson<T>(this DBComponent self, string json, string collection = null) where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
		    {
			    FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);
			    IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filterDefinition);
			    return await cursor.ToListAsync();
		    }
	    }

	    public static async ETTask<List<T>> QueryJson<T>(this DBComponent self, long taskId, string json, string collection = null) where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
		    {
			    FilterDefinition<T> filterDefinition = new JsonFilterDefinition<T>(json);
			    IAsyncCursor<T> cursor = await self.GetCollection<T>(collection).FindAsync(filterDefinition);
			    return await cursor.ToListAsync();
		    }
	    }

	    #endregion

	    #region Insert

	    public static async ETTask InsertBatch<T>(this DBComponent self, IEnumerable<T> list, string collection = null) where T: Entity
	    {
		    if (collection == null)
		    {
			    collection = typeof (T).Name;
		    }
		    
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
		    {
			    await self.GetCollection(collection).InsertManyAsync(list);
		    }
	    }

	    #endregion

	    #region Save

	    public static async ETTask Save<T>(this DBComponent self, T entity, string collection = null) where T : Entity
	    {
		    if (entity == null)
		    {
			    Log.Error($"save entity is null: {typeof (T).Name}");

			    return;
		    }
		    
		    if (collection == null)
		    {
			    collection = entity.GetType().Name;
		    }

		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, entity.Id % DBComponent.TaskCount))
		    {
			    await self.GetCollection(collection).ReplaceOneAsync(d => d.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
		    }
	    }

	    public static async ETTask Save<T>(this DBComponent self, long taskId, T entity, string collection = null) where T : Entity
	    {
		    if (entity == null)
		    {
			    Log.Error($"save entity is null: {typeof (T).Name}");

			    return;
		    }

		    if (collection == null)
		    {
			    collection = entity.GetType().Name;
		    }

		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, taskId % DBComponent.TaskCount))
		    {
			    await self.GetCollection(collection).ReplaceOneAsync(d => d.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
		    }
	    }

	    public static async ETTask Save(this DBComponent self, long id, List<Entity> entities)
	    {
		    if (entities == null)
		    {
			    Log.Error($"save entity is null");
			    return;
		    }

		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, id % DBComponent.TaskCount))
		    {
			    foreach (Entity entity in entities)
			    {
				    if (entity == null)
				    {
					    continue;
				    }

				    await self.GetCollection(entity.GetType().Name)
						    .ReplaceOneAsync(d => d.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
			    }
		    }
	    }

	    public static async ETVoid SaveNotWait<T>(this DBComponent self, T entity, long taskId = 0, string collection = null) where T : Entity
	    {
		    if (taskId == 0)
		    {
			    await self.Save(entity, collection);

			    return;
		    }

		    await self.Save(taskId, entity, collection);
	    }

	    #endregion

	    #region Remove
	    
	    public static async ETTask<long> Remove<T>(this DBComponent self, long id, string collection = null) where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, id % DBComponent.TaskCount))
		    {
			    DeleteResult result = await self.GetCollection<T>(collection).DeleteOneAsync(d => d.Id == id);

			    return result.DeletedCount;
		    }
	    }

	    public static async ETTask<long> Remove<T>(this DBComponent self, long taskId, long id, string collection = null) where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, taskId % DBComponent.TaskCount))
		    {
			    DeleteResult result = await self.GetCollection<T>(collection).DeleteOneAsync(d => d.Id == id);

			    return result.DeletedCount;
		    }
	    }

	    public static async ETTask<long> Remove<T>(this DBComponent self, Expression<Func<T, bool>> filter, string collection = null) where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
		    {
			    DeleteResult result = await self.GetCollection<T>(collection).DeleteManyAsync(filter);

			    return result.DeletedCount;
		    }
	    }

	    public static async ETTask<long> Remove<T>(this DBComponent self, long taskId, Expression<Func<T, bool>> filter, string collection = null)
			    where T : Entity
	    {
		    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, taskId % DBComponent.TaskCount))
		    {
			    DeleteResult result = await self.GetCollection<T>(collection).DeleteManyAsync(filter);

			    return result.DeletedCount;
		    }
	    }

	    #endregion
    }
}