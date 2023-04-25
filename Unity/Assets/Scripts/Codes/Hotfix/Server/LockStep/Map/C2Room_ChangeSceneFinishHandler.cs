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
            roomPlayer.IsJoinRoom = true;
            roomServerComponent.AlreadyJoinRoomCount++;

            if (roomServerComponent.AlreadyJoinRoomCount <= LSConstValue.MatchCount)
            {
                // 通知给已加进来的客户端每个玩家的进度
            }

            if (roomServerComponent.AlreadyJoinRoomCount == LSConstValue.MatchCount)
            {
                await TimerComponent.Instance.WaitAsync(1000);

                Room2C_Start room2CStart = new() {StartTime = TimeHelper.ServerFrameTime()};
                foreach (RoomPlayer rp in roomServerComponent.Children.Values)
                {
                    room2CStart.UnitInfo.Add(new LockStepUnitInfo()
                    {
                        PlayerId = rp.Id, 
                        Position = new TSVector(20, 0, -10), 
                        Rotation = TSQuaternion.identity
                    });
                }
                
                room.Init(room2CStart);
                
                room.AddComponent<RoomServerUpdater>();

                RoomMessageHelper.BroadCast(room, room2CStart);
            }

            await ETTask.CompletedTask;
        }
    }
}