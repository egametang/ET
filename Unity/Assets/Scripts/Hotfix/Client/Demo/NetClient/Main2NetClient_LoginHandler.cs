using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    [ActorMessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginHandler: ActorMessageHandler<Scene, Main2NetClient_Login, NetClient2Main_Login>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_Login request, NetClient2Main_Login response)
        {
            string account = request.Account;
            string password = request.Password;
            // 创建一个ETModel层的Session
            root.RemoveComponent<RouterAddressComponent>();
            // 获取路由跟realmDispatcher地址
            RouterAddressComponent routerAddressComponent = root.GetComponent<RouterAddressComponent>();
            if (routerAddressComponent == null)
            {
                routerAddressComponent =
                        root.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                await routerAddressComponent.Init();
                root.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
            }
            IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

            R2C_Login r2CLogin;
            using (Session session = await RouterHelper.CreateRouterSession(root, realmAddress))
            {
                r2CLogin = (R2C_Login)await session.Call(new C2R_Login() { Account = account, Password = password });
            }

            // 创建一个gate Session,并且保存到SessionComponent中
            Session gateSession = await RouterHelper.CreateRouterSession(root, NetworkHelper.ToIPEndPoint(r2CLogin.Address));
            gateSession.AddComponent<ClientSessionErrorComponent>();
            root.AddComponent<SessionComponent>().Session = gateSession;
            G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId });

            Log.Debug("登陆gate成功!");

            response.PlayerId = g2CLoginGate.PlayerId;
        }
    }
}