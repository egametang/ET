using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class R2G_GetLoginKey_Hander : AMRpcHandler<R2G_GetLoginKey_Request, G2R_GetLoginKey_Response>
    {
        protected override async void Run(Session session, R2G_GetLoginKey_Request message, Action<G2R_GetLoginKey_Response> reply)
        {
            G2R_GetLoginKey_Response response = new G2R_GetLoginKey_Response();
            try
            {
                long key = RandomHelper.RandInt64();
                Game.Scene.GetComponent<CG_GateSessionKeyComponent>().Add(key, message.UserID);
                response.Key = key;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}