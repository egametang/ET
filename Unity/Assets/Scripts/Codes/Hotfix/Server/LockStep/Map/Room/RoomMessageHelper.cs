namespace ET.Server
{

    public static class RoomMessageHelper
    {
        public static void BroadCast(Scene room, IActorMessage message)
        {
            RoomComponent roomComponent = room.GetComponent<RoomComponent>();
            foreach (var kv in roomComponent.Children)
            {
                RoomPlayer roomPlayer = kv.Value as RoomPlayer;
                ActorMessageSenderComponent.Instance.Send(roomPlayer.SessionInstanceId, message);
            }
        }
    }
}