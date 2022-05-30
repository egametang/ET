namespace ET
{
    [ObjectSystem]
    public class ModeContexAwakeSystem: AwakeSystem<ModeContex>
    {
        public override void Awake(ModeContex self)
        {
            self.Mode = "";
        }
    }

    [ObjectSystem]
    public class ModeContexDestroySystem: DestroySystem<ModeContex>
    {
        public override void Destroy(ModeContex self)
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