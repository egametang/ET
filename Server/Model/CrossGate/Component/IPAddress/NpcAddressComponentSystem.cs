namespace ETModel
{
    [ObjectSystem]
    public class NpcAddressComponentSystem : StartSystem<NpcAddressComponent>
    {
        public override void Start(NpcAddressComponent self)
        {
            self.Start();
        }
    }

    public static class NpcAddressComponentEx
    {
        public static void Start(this NpcAddressComponent component)
        {
            StartConfig[] startConfigs = component.Entity.GetComponent<StartConfigComponent>().GetAll();
            foreach (StartConfig config in startConfigs)
            {
                if (!config.AppType.Is(AppType.Npc))
                {
                    continue;
                }
                component.GateAddress.Add(config);
            }
        }
    }
}
