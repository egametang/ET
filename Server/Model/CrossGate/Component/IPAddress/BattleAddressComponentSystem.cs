namespace ETModel
{
    [ObjectSystem]
    public class BattleAddressComponentSystem : StartSystem<BattleAddressComponent>
    {
        public override void Start(BattleAddressComponent self)
        {
            self.Start();
        }
    }

    public static class BattleAddressComponentEx
    {
        public static void Start(this BattleAddressComponent component)
        {
            StartConfig[] startConfigs = component.Entity.GetComponent<StartConfigComponent>().GetAll();
            foreach (StartConfig config in startConfigs)
            {
                if (!config.AppType.Is(AppType.Battle))
                {
                    continue;
                }
                component.GateAddress.Add(config);
            }
        }
    }
}
