namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            //重新登录的时候，重新初始化NetClientComponent
            root.RemoveComponent<ClientSenderComponent>();
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            await clientSenderComponent.OnAwake();
            //重新初始化剩下的数据模块
            root.RemoveComponent<LoginModuleComponent>();
            var loginModuleComponent = root.AddComponent<LoginModuleComponent>();
            //test
            long playerId = await loginModuleComponent.LoginAsync(account, password);
            Log.Debug("playerId=" + playerId);
            var exp = await loginModuleComponent.AddExpAsync(120);
            Log.Debug("exp=" + exp);

            root.GetComponent<PlayerComponent>().MyId = playerId;
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}