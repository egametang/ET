using System;

namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene zoneScene, string account, string password)
        {
            try
            {
                // 创建一个ETModel层的Session
                R2C_Login r2CLogin;
                Session session = null;
                try
                {
                    zoneScene.RemoveComponent<RouterAddressComponent>();
                    // 获取路由跟realmDispatcher地址
                    RouterAddressComponent routerAddressComponent = zoneScene.GetComponent<RouterAddressComponent>();
                    if (routerAddressComponent == null)
                    {
                        routerAddressComponent = zoneScene.AddComponent<RouterAddressComponent, string>(ConstValue.RouterHttpAddress);
                        await routerAddressComponent.Init();
                    }
                    string realmAddress = routerAddressComponent.GetRealmAddress(account);
                    
                    session = await RouterHelper.CreateRouterSession(zoneScene, realmAddress);
                    {
                        r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = password });
                    }
                }
                finally
                {
                    session?.Dispose();
                }

                // 创建一个gate Session,并且保存到SessionComponent中
                Session gateSession = await RouterHelper.CreateRouterSession(zoneScene, r2CLogin.Address);
                zoneScene.AddComponent<SessionComponent>().Session = gateSession;
				
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});

                Log.Debug("登陆gate成功!");

                await Game.EventSystem.PublishAsync(zoneScene, new EventType.LoginFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        } 
    }
}