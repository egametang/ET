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

    public class ModeContex: Entity
    {
        public string Mode = "";
    }
}