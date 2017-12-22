using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Gate)]
    public class LoginGateRtHandler : AMRpcHandler<LoginGateRt, LoginGateRe>
    {
        protected override async void Run(Session session, LoginGateRt message, Action<LoginGateRe> reply)
        {
            LoginGateRe response = new LoginGateRe();
            try
            {
                long userId = Game.Scene.GetComponent<GateSessionKeyComponent>().Get(message.Key);

                //验证登录Key是否正确
                if (userId == 0)
                {
                    response.Error = ErrorCode.ERR_ConnectGateKeyError;
                    reply(response);
                    return;
                }

                //添加到玩家管理
                Player player = EntityFactory.Create<Player, long>(userId);
                player.AddComponent<UnitGateComponent, long>(session.Id);
                Game.Scene.GetComponent<PlayerComponent>().Add(player);

                //添加玩家连接检测组件
                session.AddComponent<SessionPlayerComponent, Player>(player);

                //添加消息转发组件
                await session.AddComponent<ActorComponent, IEntityActorHandler>(new GateSessionEntityActorHandler()).AddLocation();

                Log.Info($"玩家{userId}上线");

                response.PlayerId = player.Id;
                response.UserId = player.UserId;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
