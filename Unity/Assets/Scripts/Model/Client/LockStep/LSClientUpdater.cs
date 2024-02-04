using TrueSync;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSClientUpdater: Entity, IAwake, IUpdate
    {
        public LSInput Input = new();
        
        public long MyId { get; set; }
    }
}