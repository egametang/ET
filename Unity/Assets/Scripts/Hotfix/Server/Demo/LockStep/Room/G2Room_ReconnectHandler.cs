using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.RoomRoot)]
    public class G2Room_ReconnectHandler: MessageHandler<Scene, G2Room_Reconnect, Room2G_Reconnect>
    {
        protected override async ETTask Run(Scene root, G2Room_Reconnect request, Room2G_Reconnect response)
        {
            Room room = root.GetComponent<Room>();
            response.StartTime = room.StartTime;
            LSUnitComponent lsUnitComponent = room.LSWorld.GetComponent<LSUnitComponent>();
            foreach (long playerId in room.PlayerIds)
            {
                LSUnit lsUnit = lsUnitComponent.GetChild<LSUnit>(playerId);
                LockStepUnitInfo lockStepUnitInfo = LockStepUnitInfo.Create();
                lockStepUnitInfo.PlayerId = playerId;
                lockStepUnitInfo.Position = lsUnit.Position;
                lockStepUnitInfo.Rotation = lsUnit.Rotation;
                response.UnitInfos.Add(lockStepUnitInfo);    
            }

            response.Frame = room.AuthorityFrame;
            await ETTask.CompletedTask;
        }
    }
}