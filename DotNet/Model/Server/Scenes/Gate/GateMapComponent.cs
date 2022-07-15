namespace ET.Server
{
    [ComponentOf(typeof(Player))]
    public class GateMapComponent: Entity, IAwake
    {
        public Scene Scene { get; set; }
    }
}