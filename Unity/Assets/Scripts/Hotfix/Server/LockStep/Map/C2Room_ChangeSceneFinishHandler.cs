using System.Collections.Generic;
using TrueSync;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    [FriendOf(typeof (RoomServerComponent))]
    public class C2Room_ChangeSceneFinishHandler: AMActorHandler<Room, C2Room_ChangeSceneFinish>
    {
        protected override async ETTask Run(Room room, C2Room_ChangeSceneFinish message)
        {
            RoomServerComponent roomServerComponent = room.GetComponent<RoomServerComponent>();
            RoomPlayer roomPlayer = room.GetComponent<RoomServerComponent>().GetChild<RoomPlayer>(message.PlayerId);
            roomPlayer.Progress = 100;
            
            if (!roomServerComponent.IsAllPlayerProgress100())
            {
                return;
            }
            
            await TimerComponent.Instance.WaitAsync(1000);

            Room2C_Start room2CStart = new() { StartTime = TimeHelper.ServerFrameTime() };
            foreach (RoomPlayer rp in roomServerComponent.Children.Values)
            {
                room2CStart.UnitInfo.Add(new LockStepUnitInfo()
                {
                    PlayerId = rp.Id, Position = new TSVector(20, 0, -10), Rotation = TSQuaternion.identity
                });
            }

            room.Init(room2CStart);

            room.AddComponent<RoomServerUpdater>();

            RoomMessageHelper.BroadCast(room, room2CStart);
        }
    }
}