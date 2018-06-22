using System;
using System.Net;
using ETModel;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_LoginGate_Handler : AMRpcHandler<C2G_LoginGate_Request, G2C_LoginGate_Response>
    {
        protected override async void Run(Session session, C2G_LoginGate_Request message, Action<G2C_LoginGate_Response> reply)
        {
            G2C_LoginGate_Response response = new G2C_LoginGate_Response();
            try
            {
                CG_GateSessionKeyComponent gateSessionKeyComponent = Game.Scene.GetComponent<CG_GateSessionKeyComponent>();
                long userId = gateSessionKeyComponent.Get(message.Key);

                //验证登录Key是否正确
                if (userId < 1)
                {
                    response.Error = ErrorCode.ERR_ConnectGateKeyError;
                    reply(response);
                    return;
                }

                //验证通过后移除Key
                gateSessionKeyComponent.Remove(message.Key);

                //创建User对象
                User user = UserFactory.Create(userId, session.Id);
                await user.AddComponent<MailBoxComponent>().AddLocation();

                //添加User对象关联到Session上, 用于Session断开时触发下线
                session.AddComponent<SessionUserComponent>().User = user;
                //添加消息转发组件
                await session.AddComponent<MailBoxComponent, string>(ActorType.GateSession).AddLocation();

                //向登录服务器发送玩家上线消息(*用于重复登录是否可以踢下线)
                StartConfigComponent config = Game.Scene.GetComponent<StartConfigComponent>();
                IPEndPoint realmIPEndPoint = config.RealmConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session realmSession = Game.Scene.GetComponent<NetInnerComponent>().Get(realmIPEndPoint);
                await realmSession.Call(new G2R_PlayerOnline_Request { UserID = userId, GateAppID = config.StartConfig.AppId });

                Log.Info("Gate登录通过: " + userId);

                //数据库操作对象, 查询帐号是否存在角色, 验证帐号请求
                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();
                Character info = await dbProxy.Query<Character>(userId);

                //把需要的参数返回给客户端
                response.PlayerID = user.Id;
                response.UserID = user.UserID;
                response.Info = info == null ? null : RoleDataConvertHelper.GetBasicRoleInfo(info);
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }

    }
}