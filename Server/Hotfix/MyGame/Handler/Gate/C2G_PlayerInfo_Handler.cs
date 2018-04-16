using System;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_PlayerInfo_Handler : AMRpcHandler<C2G_PlayerInfo, G2C_PlayerInfo>
    {
        protected override async void Run(Session session, C2G_PlayerInfo message, Action<G2C_PlayerInfo> reply)
        {
            G2C_PlayerInfo response = new G2C_PlayerInfo();
            try
            {
                //验证Session
                //if (!GateHelper.SignSession(session))
                //{
                //    response.Error = ErrorCode.ERR_SignError;
                //    reply(response);
                //    return;
                //}

                //查询用户信息
                DBProxyComponent dbProxyComponent = Game.Scene.GetComponent<DBProxyComponent>();
                UserInfo userInfo = await dbProxyComponent.Query<UserInfo>(message.UserID, false);

                response.PlayerInfos.Nickname = userInfo.NickName;
                response.PlayerInfos.Gold = userInfo.Gold;
                response.PlayerInfos.RoomCard = userInfo.RoomCard;

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
