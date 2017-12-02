using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBFindBatchRequestHandler : AMRpcHandler<DBFindBatchRequest, DBFindBatchResponse>
    {
        protected override async void Run(Session session, DBFindBatchRequest message, Action<DBFindBatchResponse> reply)
        {
            DBFindBatchResponse response = new DBFindBatchResponse();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                List<Entity> result = await dbCacheComponent.FindBatch(message.Options, message.CollectionName);

                if (message.NeedCache)
                {
                    foreach (Entity entity in result)
                    {
                        dbCacheComponent.AddToCache(entity, message.CollectionName);
                    }
                }

                response.Result = result;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
