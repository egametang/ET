using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBUpdateRequestHandler : AMRpcHandler<DBUpdateRequest, DBUpdateResponse>
    {
        protected override async void Run(Session session, DBUpdateRequest message, Action<DBUpdateResponse> reply)
        {
            DBUpdateResponse response = new DBUpdateResponse();
            try
            {
                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                await dbCacheComponent.Update(message.Options, message.CollectionName);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
