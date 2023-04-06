namespace ET.Server
{

    [ChildOf(typeof (RoomComponent))]
    public class RoomPlayer: Entity, IAwake
    {
        public long SessionInstanceId { get; set; }
        public bool IsJoinRoom { get; set; }
    }
}