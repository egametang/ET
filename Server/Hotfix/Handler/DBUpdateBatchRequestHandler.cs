using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBUpdateBatchRequestHandler : AMRpcHandler<DBUpdateBatchRequest, DBUpdateBatchResponse>
    {
        protected override async void Run(Session session, DBUpdateBatchRequest message, Action<DBUpdateBatchResponse> reply)
        {
            DBUpdateBatchResponse response = new DBUpdateBatchResponse();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                await dbCacheComponent.UpdateBatch(message.Options, message.CollectionName);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
