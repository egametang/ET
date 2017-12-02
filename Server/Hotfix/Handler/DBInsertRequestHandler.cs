using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DB)]
    public class DBInsertRequestHandler : AMRpcHandler<DBInsertRequest, DBInsertResponse>
    {
        protected override async void Run(Session session, DBInsertRequest message, Action<DBInsertResponse> reply)
        {
            DBInsertResponse response = new DBInsertResponse();
            try
            {
                if(message.Entity == null)
                {
                    throw new Exception("插入数据失败，Entity不能为空!");
                }

                DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
                await dbCacheComponent.Insert(message.Entity, message.CollectionName);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
