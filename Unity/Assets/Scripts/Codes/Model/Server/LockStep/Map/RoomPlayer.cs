namespace ET.Server
{

    [ChildOf(typeof (RoomComponent))]
    public class RoomPlayer: Entity, IAwake
    {
        public bool IsJoinRoom { get; set; }
    }
}