namespace ET.Server
{

    [ComponentOf(typeof (Player))]
    public class PlayerRoomComponent: Entity, IAwake
    {
        public long RoomInstanceId { get; set; }
    }
}