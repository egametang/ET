using System.Collections.Generic;
using TrueSync;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.RoomRoot)]
    [FriendOf(typeof (RoomServerComponent))]
    public class C2Room_ChangeSceneFinishHandler: ActorMessageHandler<Scene, C2Room_ChangeSceneFinish>
    {
        protected override async ETTask Run(Scene root, C2Room_ChangeSceneFinish message)
        {
            Room room = root.GetComponent<Room>();
            RoomServerComponent roomServerComponent = room.GetComponent<RoomServerComponent>();
            RoomPlayer roomPlayer = room.GetComponent<RoomServerComponent>().GetChild<RoomPlayer>(message.PlayerId);
            roomPlayer.Progress = 100;
            
            if (!roomServerComponent.IsAllPlayerProgress100())
            {
                return;
            }
            
            await room.Fiber.TimerComponent.WaitAsync(1000);

            Room2C_Start room2CStart = new() { StartTime = room.Fiber().TimeInfo.ServerFrameTime() };
            foreach (RoomPlayer rp in roomServerComponent.Children.Values)
            {
                room2CStart.UnitInfo.Add(new LockStepUnitInfo()
                {
                    PlayerId = rp.Id, Position = new TSVector(20, 0, -10), Rotation = TSQuaternion.identity
                });
            }

            room.Init(room2CStart.UnitInfo, room2CStart.StartTime);

            room.AddComponent<LSServerUpdater>();

            RoomMessageHelper.BroadCast(room, room2CStart);
        }
    }
}