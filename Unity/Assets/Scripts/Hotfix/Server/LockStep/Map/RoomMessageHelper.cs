namespace ET.Server
{

    public static partial class RoomMessageHelper
    {
        public static void BroadCast(Room room, IActorMessage message)
        {
            // 广播的消息不能被池回收
            (message as MessageObject).IsFromPool = false;
            
            RoomServerComponent roomServerComponent = room.GetComponent<RoomServerComponent>();

            ActorLocationSenderComponent actorLocationSenderComponent = room.Root().GetComponent<ActorLocationSenderComponent>();
            foreach (var kv in roomServerComponent.Children)
            {
                RoomPlayer roomPlayer = kv.Value as RoomPlayer;

                if (!roomPlayer.IsOnline)
                {
                    continue;
                }
                
                actorLocationSenderComponent.Get(LocationType.GateSession).Send(roomPlayer.Id, message);
            }
        }
    }
}