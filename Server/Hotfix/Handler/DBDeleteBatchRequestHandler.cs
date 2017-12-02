using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBDeleteBatchRequestHandler : AMRpcHandler<DBDeleteBatchRequest, DBDeleteBatchResponse>
    {
        protected override async void Run(Session session, DBDeleteBatchRequest message, Action<DBDeleteBatchResponse> reply)
        {
            DBDeleteBatchResponse response = new DBDeleteBatchResponse();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                await dbCacheComponent.DeleteBatch(message.Filter, message.CollectionName);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
