using System;
using System.Net;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Gate)]
    public class StartMatchRtHandler : AMRpcHandler<StartMatchRt, StartMatchRe>
    {
        protected override async void Run(Session session, StartMatchRt message, Action<StartMatchRe> reply)
        {
            StartMatchRe response = new StartMatchRe();
            try
            {
                //验证玩家ID是否正常
                Player player = Game.Scene.GetComponent<PlayerComponent>().Get(message.PlayerId);
                if (player == null)
                {
                    response.Error = ErrorCode.ERR_AccountOrPasswordError;
                    reply(response);
                    return;
                }

                //验证玩家是否符合进入房间要求
                RoomConfig roomConfig = RoomHelper.GetConfig(message.Level);
                UserInfo userInfo = await Game.Scene.GetComponent<DBProxyComponent>().Query<UserInfo>(player.UserId);
                if (userInfo.Money < roomConfig.MinThreshold)
                {
                    response.Error = ErrorCode.ErrUserMoneyLessError;
                    reply(response);
                    return;
                }

                //向匹配服务器发送匹配请求
                StartConfigComponent config = Game.Scene.GetComponent<StartConfigComponent>();
                IPEndPoint matchAddress = config.MatchConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session matchSession = Game.Scene.GetComponent<NetInnerComponent>().Get(matchAddress);
                JoinMatchRe joinMatchRe = await matchSession.Call<JoinMatchRe>(new JoinMatchRt()
                {
                    PlayerId = player.Id,
                    UserId = player.UserId,
                    GateSessionId = session.Id,
                    GateAppId = config.StartConfig.AppId
                });

                //设置玩家的Actor消息直接发送给匹配对象
                player.ActorId = joinMatchRe.ActorId;

                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
