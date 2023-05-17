using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class G2Room_ReconnectHandler: AMActorRpcHandler<Room, G2Room_Reconnect, Room2G_Reconnect>
    {
        protected override async ETTask Run(Room room, G2Room_Reconnect request, Room2G_Reconnect response)
        {
            response.StartTime = room.StartTime;
            response.UnitInfos = new List<LockStepUnitInfo>();
            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            foreach (long playerId in room.PlayerIds)
            {
                LSUnit lsUnit = lsUnitComponent.GetChild<LSUnit>(playerId);
                response.UnitInfos.Add(new LockStepUnitInfo() {PlayerId = playerId, Position = lsUnit.Position, Rotation = lsUnit.Rotation});    
            }

            response.Frame = room.AuthorityFrame;
            await ETTask.CompletedTask;
        }
    }
}