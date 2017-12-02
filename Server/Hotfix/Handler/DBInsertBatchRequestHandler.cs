using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBInsertBatchRequestHandler : AMRpcHandler<DBInsertBatchRequest, DBInsertBatchResponse>
    {
        protected override async void Run(Session session, DBInsertBatchRequest message, Action<DBInsertBatchResponse> reply)
        {
            DBInsertBatchResponse response = new DBInsertBatchResponse();
            try
            {
                if (message.Entitys == null || message.Entitys.Count == 0)
                {
                    throw new Exception("批量插入数据失败，Entitys不能为空!");
                }

                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                await dbCacheComponent.InsertBatch(message.Entitys, message.CollectionName);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
