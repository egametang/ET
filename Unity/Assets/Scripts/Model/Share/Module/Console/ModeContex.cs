namespace ET
{
    [FriendOf(typeof(ModeContex))]
    public static class ModeContexSystem
    {
        [EntitySystem]
        public class ModeContexAwakeSystem: AwakeSystem<ModeContex>
        {
            protected override void Awake(ModeContex self)
            {
                self.Awake();
            }
        }

        [EntitySystem]
        public class ModeContexDestroySystem: DestroySystem<ModeContex>
        {
            protected override void Destroy(ModeContex self)
            {
                self.Destroy();
            }
        }

        private static void Awake(this ModeContex self)
        {
            self.Mode = "";
        }
        
        private static void Destroy(this ModeContex self)
        {
            self.Mode = "";
        }
    }
    


    [ComponentOf(typeof(ConsoleComponent))]
    public class ModeContex: Entity, IAwake, IDestroy
    {
        public string Mode = "";
    }
}