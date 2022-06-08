namespace ET.Client
{
    [ObjectSystem]
    public class ClientSceneFlagComponentDestroySystem: DestroySystem<ClientSceneFlagComponent>
    {
        public override void Destroy(ClientSceneFlagComponent self)
        {
            ClientSceneManagerComponent.Instance.Remove(self.DomainZone());
        }
    }

    [ObjectSystem]
    public class ClientSceneFlagComponentAwakeSystem: AwakeSystem<ClientSceneFlagComponent>
    {
        public override void Awake(ClientSceneFlagComponent self)
        {
            ClientSceneManagerComponent.Instance.Add(self.GetParent<Scene>());
        }
    }
}