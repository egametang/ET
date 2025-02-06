namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class PlayerComponent: Entity, IAwake
    {
        public long MyId { get; set; }
    }
}