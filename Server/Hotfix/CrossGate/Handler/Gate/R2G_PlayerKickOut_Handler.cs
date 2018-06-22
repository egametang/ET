using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class R2G_PlayerKickOut_Handler : AMRpcHandler<R2G_PlayerKickOut_Request, G2R_PlayerKickOut_Response>
    {
        protected override async void Run(Session session, R2G_PlayerKickOut_Request message, Action<G2R_PlayerKickOut_Response> reply)
        {
            G2R_PlayerKickOut_Response response = new G2R_PlayerKickOut_Response();
            try
            {
                User user = Game.Scene.GetComponent<UserComponent>().Get(message.UserID);
                //服务端主动断开客户端连接
                long userSessionId = user.GetComponent<UnitGateComponent>().GateSessionActorId;
                Game.Scene.GetComponent<NetOuterComponent>().Remove(userSessionId);
                Log.Info($"已将玩家{message.UserID}踢下线并断开连接");

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
