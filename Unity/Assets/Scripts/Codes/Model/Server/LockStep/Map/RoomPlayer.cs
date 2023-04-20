namespace ET.Server
{

    [ChildOf(typeof (RoomServerComponent))]
    public class RoomPlayer: Entity, IAwake
    {
        public bool IsJoinRoom { get; set; }
        public int Slot { get; set; }
    }
}