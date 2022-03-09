using System;


namespace ET
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene zoneScene, string address, string account, string password)
        {
            try
            {
                // 创建一个ETModel层的Session
                R2C_Login r2CLogin;
                Session session = null;
                try
                {
                    session = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(address));
                    {
                        r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = password });
                    }
                }
                finally
                {
                    session?.Dispose();
                }

                // 创建一个gate Session,并且保存到SessionComponent中
                Session gateSession = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(r2CLogin.Address));
                gateSession.AddComponent<PingComponent>();
                zoneScene.AddComponent<SessionComponent>().Session = gateSession;
				
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)
                        await gateSession.Call( new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});
                      
                Log.Debug("登陆gate成功!");

                await Game.EventSystem.PublishAsync(new EventType.LoginFinish() {ZoneScene = zoneScene});
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        public static async ETTask LoginTest(Scene zoneScene, string address)
        {
            try
            {
                Session session = null;
                R2C_LoginTest r2CLoginTest = null;

                try
                {
                     session = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(address));
                     
                     r2CLoginTest = (R2C_LoginTest)await session.Call(new C2R_LoginTest() { Account = "110", Password = "110" });
                     Log.Debug(r2CLoginTest.Message);
                     
                     session.Send(new C2R_SayHello(){Content = "你好啊 服务器"});
                     await TimerComponent.Instance.WaitAsync(2000);
                }
                finally
                {
                    session.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}