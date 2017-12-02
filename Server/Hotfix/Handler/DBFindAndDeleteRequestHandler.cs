using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBFindAndDeleteRequestHandler : AMRpcHandler<DBFindAndDeleteRequest, DBFindAndDeleteResponse>
    {
        protected override async void Run(Session session, DBFindAndDeleteRequest message, Action<DBFindAndDeleteResponse> reply)
        {
            DBFindAndDeleteResponse response = new DBFindAndDeleteResponse();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                Entity result = await dbCacheComponent.FindAndDelete(message.Options, message.CollectionName);

                if (result != null && !string.IsNullOrEmpty(message.CollectionName))
                {
                    dbCacheComponent.RemoveFromCache(message.CollectionName, result.Id);
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
