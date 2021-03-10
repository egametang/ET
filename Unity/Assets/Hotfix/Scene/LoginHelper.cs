using System;


namespace ET
{
    public static class LoginHelper
    {
        public static async ETVoid Login(Scene zoneScene, string address, string account)
        {
            try
            {
                // 创建一个ETModel层的Session
                R2C_Login r2CLogin;
                using (Session session = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(address)))
                {
                    r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = "111111" });
                }

                // 创建一个gate Session,并且保存到SessionComponent中
                Session gateSession = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(r2CLogin.Address));
                gateSession.AddComponent<PingComponent>();
                zoneScene.AddComponent<SessionComponent>().Session = gateSession;
				
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});

                Log.Info("登陆gate成功!");

                await Game.EventSystem.Publish(new EventType.LoginFinish() {ZoneScene = zoneScene});
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        } 
    }
}