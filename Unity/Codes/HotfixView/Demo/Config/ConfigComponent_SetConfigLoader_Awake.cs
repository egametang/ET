namespace ET
{
    public class ConfigComponent_SetConfigLoader_Awake: AwakeSystem<ConfigComponent>
    {
        public override void Awake(ConfigComponent self)
        {
            self.ConfigLoader = new ConfigLoader();
        }
    }
}