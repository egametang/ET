namespace ET.Server
{
    [EntitySystemOf(typeof(ModeContex))]
    public static partial class ModeContexSystem
    {
        [EntitySystem]
        private static void Awake(this ModeContex self)
        {
            self.Mode = "";
        }
        
        [EntitySystem]
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