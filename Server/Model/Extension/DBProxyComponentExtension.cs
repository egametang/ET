using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model
{
    public static class DBProxyComponentExtension
    {
        public static async Task Insert(this DBProxyComponent self, Entity entity, bool needCache = false, string collectionName = "")
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            await session.Call<DBInsertResponse>(new DBInsertRequest { Entity = entity, NeedCache = needCache, CollectionName = collectionName });
        }

        public static async Task InsertBatch(this DBProxyComponent self, List<Entity> entitys, bool needCache = false, string collectionName = "")
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            await session.Call<DBInsertBatchResponse>(new DBInsertBatchRequest { Entitys = entitys, NeedCache = needCache, CollectionName = collectionName });
        }

        public static async Task Delete(this DBProxyComponent self, string filter, string collectionName)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            await session.Call<DBDeleteResponse>(new DBDeleteRequest { Filter = filter, CollectionName = collectionName });
        }

        public static async Task DeleteBatch(this DBProxyComponent self, string filter, string collectionName)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            await session.Call<DBDeleteBatchResponse>(new DBDeleteBatchRequest { Filter = filter, CollectionName = collectionName });
        }

        public static async Task Update(this DBProxyComponent self, DBUpdateOptions options, string collectionName)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            await session.Call<DBUpdateResponse>(new DBUpdateRequest { Options = options, CollectionName = collectionName });
        }

        public static async Task UpdateBatch(this DBProxyComponent self, DBUpdateOptions options, string collectionName)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            await session.Call<DBUpdateBatchResponse>(new DBUpdateBatchRequest { Options = options, CollectionName = collectionName });
        }

        public static async Task<Entity> Find(this DBProxyComponent self, DBFindOptions options, string collectionName, bool needCache = false)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            DBFindResponse response = await session.Call<DBFindResponse>(new DBFindRequest { Options = options, NeedCache = needCache, CollectionName = collectionName });
            return response.Result;
        }

        public static async Task<List<Entity>> FindBatch(this DBProxyComponent self, DBFindOptions options, string collectionName, bool needCache = false)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            DBFindBatchResponse response = await session.Call<DBFindBatchResponse>(new DBFindBatchRequest { Options = options, NeedCache = needCache, CollectionName = collectionName });
            return response.Result;
        }

        public static async Task<Entity> FindAndDelete(this DBProxyComponent self, DBFindAndDeleteOptions options, string collectionName)
        {
            Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.dbAddress);
            DBFindAndDeleteResponse response = await session.Call<DBFindAndDeleteResponse>(new DBFindAndDeleteRequest { Options = options, CollectionName = collectionName });
            return response.Result;
        }
    }
}
