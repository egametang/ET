namespace ETModel
{
    [ObjectSystem]
    public class LoginAddressComponentSystem : StartSystem<LoginAddressComponent>
    {
        public override void Start(LoginAddressComponent self)
        {
            self.Start();
        }
    }

    public static class LoginAddressComponentEx
    {
        public static void Start(this LoginAddressComponent component)
        {
            StartConfig[] startConfigs = component.Entity.GetComponent<StartConfigComponent>().GetAll();
            foreach (StartConfig config in startConfigs)
            {
                if (!config.AppType.Is(AppType.Login))
                {
                    continue;
                }
                component.GateAddress.Add(config);
            }
        }
    }
}
