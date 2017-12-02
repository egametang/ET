using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBFindRequestHandler : AMRpcHandler<DBFindRequest, DBFindResponse>
    {
        protected override async void Run(Session session, DBFindRequest message, Action<DBFindResponse> reply)
        {
            DBFindResponse response = new DBFindResponse();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                Entity result = await dbCacheComponent.Find(message.Options, message.CollectionName);

                if (message.NeedCache && result != null)
                {
                    dbCacheComponent.AddToCache(result, message.CollectionName);
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
