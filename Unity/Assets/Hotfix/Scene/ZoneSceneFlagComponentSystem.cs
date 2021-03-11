namespace ET
{
    [ObjectSystem]
    public class ZoneSceneFlagComponentDestroySystem: DestroySystem<ZoneSceneFlagComponent>
    {
        public override void Destroy(ZoneSceneFlagComponent self)
        {
            ZoneSceneManagerComponent.Instance.Remove(self.DomainZone());
        }
    }

    [ObjectSystem]
    public class ZoneSceneFlagComponentAwakeSystem: AwakeSystem<ZoneSceneFlagComponent>
    {
        public override void Awake(ZoneSceneFlagComponent self)
        {
            ZoneSceneManagerComponent.Instance.Add(self.GetParent<Scene>());
        }
    }
}