namespace ET.Server
{

    public static partial class RoomMessageHelper
    {
        public static void BroadCast(Room room, IActorMessage message)
        {
            RoomServerComponent roomServerComponent = room.GetComponent<RoomServerComponent>();
            
            foreach (var kv in roomServerComponent.Children)
            {
                RoomPlayer roomPlayer = kv.Value as RoomPlayer;

                if (!roomPlayer.IsOnline)
                {
                    continue;
                }
                
                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(roomPlayer.Id, message);
            }
        }
    }
}