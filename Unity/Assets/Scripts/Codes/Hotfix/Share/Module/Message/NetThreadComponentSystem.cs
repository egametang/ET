namespace ET
{
    [FriendOf(typeof(NetThreadComponent))]
    public static class NetThreadComponentSystem
    {
        [ObjectSystem]
        public class NetThreadComponentAwakeSystem: AwakeSystem<NetThreadComponent>
        {
            protected override void Awake(NetThreadComponent self)
            {
                NetThreadComponent.Instance = self;
                
                self.ThreadSynchronizationContext = ThreadSynchronizationContext.Instance;
            }
        }

        [ObjectSystem]
        public class NetThreadComponentUpdateSystem: LateUpdateSystem<NetThreadComponent>
        {
            protected override void LateUpdate(NetThreadComponent self)
            {
                NetServices.Instance.Update();
            }
        }
    }
}