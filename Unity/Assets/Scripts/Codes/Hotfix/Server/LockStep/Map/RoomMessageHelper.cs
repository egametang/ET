namespace ET.Server
{

    public static class RoomMessageHelper
    {
        public static void BroadCast(Scene room, IActorLocationMessage message)
        {
            RoomComponent roomComponent = room.GetComponent<RoomComponent>();
            foreach (var kv in roomComponent.Children)
            {
                RoomPlayer roomPlayer = kv.Value as RoomPlayer;
                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(roomPlayer.Id, message);
            }
        }
    }
}