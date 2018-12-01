using System;
using ETModel;

namespace ETHotfix
{
    public static class LoginHelper
    {
        public static async ETVoid OnLoginAsync(string account)
        {
            try
            {
                // 创建一个ETModel层的Session
                ETModel.Session session = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(GlobalConfigComponent.Instance.GlobalProto.Address);
				
                // 创建一个ETHotfix层的Session, ETHotfix的Session会通过ETModel层的Session发送消息
                Session realmSession = ComponentFactory.Create<Session, ETModel.Session>(session);
                R2C_Login r2CLogin = (R2C_Login) await realmSession.Call(new C2R_Login() { Account = account, Password = "111111" });
                realmSession.Dispose();

                // 创建一个ETModel层的Session,并且保存到ETModel.SessionComponent中
                ETModel.Session gateSession = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(r2CLogin.Address);
                ETModel.Game.Scene.AddComponent<ETModel.SessionComponent>().Session = gateSession;
				
                // 创建一个ETHotfix层的Session, 并且保存到ETHotfix.SessionComponent中
                Game.Scene.AddComponent<SessionComponent>().Session = ComponentFactory.Create<Session, ETModel.Session>(gateSession);
				
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2CLogin.Key });

                Log.Info("登陆gate成功!");

                // 创建Player
                Player player = ETModel.ComponentFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
                PlayerComponent playerComponent = ETModel.Game.Scene.GetComponent<PlayerComponent>();
                playerComponent.MyPlayer = player;

                Game.EventSystem.Run(EventIdType.LoginFinish);

                // 测试消息有成员是class类型
                G2C_PlayerInfo g2CPlayerInfo = (G2C_PlayerInfo) await SessionComponent.Instance.Session.Call(new C2G_PlayerInfo());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        } 
    }
}