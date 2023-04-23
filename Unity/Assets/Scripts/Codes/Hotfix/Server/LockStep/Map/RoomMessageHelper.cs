namespace ET.Server
{

    public static class RoomMessageHelper
    {
        public static void BroadCast(Room room, IActorMessage message)
        {
            RoomServerComponent roomServerComponent = room.GetComponent<RoomServerComponent>();
            
            foreach (var kv in roomServerComponent.Children)
            {
                RoomPlayer roomPlayer = kv.Value as RoomPlayer;

                if (!roomPlayer.IsJoinRoom)
                {
                    continue;
                }
                
                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(roomPlayer.Id, message);
            }
        }
    }
}