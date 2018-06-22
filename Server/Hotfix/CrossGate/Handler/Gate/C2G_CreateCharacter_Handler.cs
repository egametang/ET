using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_CreateCharacter_Handler : AMRpcHandler<C2G_CrateCharacter_Request, G2C_CrateCharacter_Response>
    {
        protected override async void Run(Session session, C2G_CrateCharacter_Request message, Action<G2C_CrateCharacter_Response> reply)
        {
            G2C_CrateCharacter_Response response = new G2C_CrateCharacter_Response();
            try
            {
                //验证Session
                if (!GateHelper.SignSession(session))
                {
                    response.Error = ErrorCode.ERR_SignError;
                    reply(response);
                    return;
                }

                //把玩家在Gate的真实ID传过去
                long userid = session.GetComponent<SessionUserComponent>().User.UserID;

                //随机分配对应的服务器
                StartConfig serverConfig = Game.Scene.GetComponent<LoginAddressComponent>().GetAddress();
                Session serverSession = Game.Scene.GetComponent<NetInnerComponent>().Get(serverConfig.GetComponent<InnerConfig>().IPEndPoint);

                //Gate只负责转发, 直接把LoginServer操作好的信息返回给客户端即可
                G2L_CreateCharacter_Request newrequest = new G2L_CreateCharacter_Request
                {
                    PlayerName = message.PlayerName,
                    CharacterID = message.CharacterID,
                    HealthBP = message.HealthBP,
                    StrBp = message.StrBp,
                    DefBP = message.DefBP,
                    SpeedBP = message.SpeedBP,
                    MagicBP = message.MagicBP,
                    Di = message.Di,
                    Shui = message.Shui,
                    Huo = message.Huo,
                    Feng = message.Feng,
                    UserID = userid,
                };

                Log.Debug("Gate把请求转发给Login");
                //Gate把请求转发给Login
                L2G_CreateCharacter_Response serverresponse = await serverSession.Call(newrequest) as L2G_CreateCharacter_Response;
                response.Error = serverresponse.Error;
                response.Info = serverresponse.Info;

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }

    }
}