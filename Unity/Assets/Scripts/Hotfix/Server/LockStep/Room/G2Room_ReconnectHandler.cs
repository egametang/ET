using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class G2Room_ReconnectHandler: ActorMessageHandler<Scene, G2Room_Reconnect, Room2G_Reconnect>
    {
        protected override async ETTask Run(Scene root, G2Room_Reconnect request, Room2G_Reconnect response)
        {
            Room room = root.GetComponent<Room>();
            response.StartTime = room.StartTime;
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