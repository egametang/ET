using ETModel;
using System;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class G2R_PlayerOffline_Handler : AMRpcHandler<G2R_PlayerOffline_Request, R2G_PlayerOffline_Response>
    {
        protected override async void Run(Session session, G2R_PlayerOffline_Request message, Action<R2G_PlayerOffline_Response> reply)
        {
            R2G_PlayerOffline_Response response = new R2G_PlayerOffline_Response();
            try
            {
                //玩家下线
                Game.Scene.GetComponent<OnlineComponent>().Remove(message.UserID);
                Log.Info($"玩家{message.UserID}下线");

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
