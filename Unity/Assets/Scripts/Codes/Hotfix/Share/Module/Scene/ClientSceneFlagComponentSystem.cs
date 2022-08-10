namespace ET
{
    [ObjectSystem]
    public class ClientSceneFlagComponentDestroySystem: DestroySystem<ClientSceneFlagComponent>
    {
        protected override void Destroy(ClientSceneFlagComponent self)
        {
            ClientSceneManagerComponent.Instance.Remove(self.DomainZone());
        }
    }

    [ObjectSystem]
    public class ClientSceneFlagComponentAwakeSystem: AwakeSystem<ClientSceneFlagComponent>
    {
        protected override void Awake(ClientSceneFlagComponent self)
        {
            ClientSceneManagerComponent.Instance.Add(self.GetParent<Scene>());
        }
    }
}