namespace ET.Server
{

    public static class RoomMessageHelper
    {
        public static void BroadCast(BattleScene battleScene, IActorMessage message)
        {
            RoomServerComponent roomServerComponent = battleScene.GetComponent<RoomServerComponent>();
            
            foreach (var kv in roomServerComponent.Children)
            {
                RoomPlayer roomPlayer = kv.Value as RoomPlayer;
                
                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(roomPlayer.Id, message);
            }
        }
    }
}