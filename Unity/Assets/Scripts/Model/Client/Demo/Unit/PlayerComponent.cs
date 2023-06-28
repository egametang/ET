namespace ET.Client
{
    [ComponentOf(typeof(Fiber))]
    public class PlayerComponent: Entity, IAwake
    {
        public long MyId { get; set; }
    }
}