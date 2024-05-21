namespace ET.Server
{
    
    [ChildOf(typeof(MessageLocationSenderComponent))]
    public class MessageLocationSenderOneType: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 60 * 1000;

        public long CheckTimer;
    }
    
    
    [ComponentOf(typeof(Scene))]
    public class MessageLocationSenderComponent: Entity, IAwake
    {
        public long CheckTimer;
    }
}