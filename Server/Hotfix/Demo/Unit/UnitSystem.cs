namespace ET
{
    public class UnitSystem: AwakeSystem<Unit, int>
    {
        public override void Awake(Unit self, int configId)
        {
            self.ConfigId = configId;
        }
    }
}