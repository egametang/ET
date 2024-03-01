namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            ClientSenderComponent clientSenderComponent = root.GetComponent<ClientSenderComponent>();
            if (clientSenderComponent != null)
            {
                await clientSenderComponent.DisposeAsync();
            }
            
            clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            
            long playerId = await clientSenderComponent.LoginAsync(account, password);

            root.GetComponent<PlayerComponent>().MyId = playerId;
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}