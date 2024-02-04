namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSReplayUpdater: Entity, IAwake, IUpdate
    {
        public int ReplaySpeed { get; set; } = 1;
    }
}