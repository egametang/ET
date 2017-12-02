using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBDeleteRequestHandler : AMRpcHandler<DBDeleteRequest, DBDeleteResponse>
    {
        protected override async void Run(Session session, DBDeleteRequest message, Action<DBDeleteResponse> reply)
        {
            DBDeleteResponse response = new DBDeleteResponse();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                await dbCacheComponent.Delete(message.Filter, message.CollectionName);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
