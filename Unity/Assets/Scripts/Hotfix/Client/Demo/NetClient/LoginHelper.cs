namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            var clientSenderComponent = root.GetComponent<ClientSenderComponent>();
            if (clientSenderComponent != null)
                await clientSenderComponent.DisposeClientSender();
            clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            var response = await clientSenderComponent.LoginAsync(account, password);
            if (response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"登录失败 {response.Error} " );
                await clientSenderComponent.DisposeClientSender();
                return;
            }
            root.GetComponent<PlayerComponent>().MyId = response.PlayerId;
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}