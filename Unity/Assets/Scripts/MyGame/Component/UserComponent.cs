namespace ETModel
{
    [ObjectSystem]
    public class UserComponentAwakeSystem : AwakeSystem<UserComponent>
    {
        public override void Awake(UserComponent self)
        {
            self.Awake();
        }
    }

    public class UserComponent : Component
    {
        public static UserComponent Instance { get; private set; }

        public User LocalPlayer { get; set; }

        public void Awake()
        {
            Instance = this;
        }
    }
}
