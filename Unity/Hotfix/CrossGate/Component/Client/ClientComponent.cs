using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ClientComponentAwakeSystem : AwakeSystem<ClientComponent>
    {
        public override void Awake(ClientComponent self)
        {
            self.Awake();
        }
    }

    public class ClientComponent : Component
    {
        public static ClientComponent Instance { get; private set; }

        public User LocalUser { get; set; }

        public Role LocalRole { get; set; }


        public void Awake()
        {
            Instance = this;
        }
    }
}
