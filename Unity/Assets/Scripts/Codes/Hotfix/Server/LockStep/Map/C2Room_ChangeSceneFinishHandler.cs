using System.Collections.Generic;
using TrueSync;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    [FriendOf(typeof (RoomComponent))]
    public class C2Room_ChangeSceneFinishHandler: AMActorHandler<Scene, C2Room_ChangeSceneFinish>
    {
        protected override async ETTask Run(Scene room, C2Room_ChangeSceneFinish message)
        {
            RoomComponent roomComponent = room.GetComponent<RoomComponent>();
            RoomPlayer roomPlayer = room.GetComponent<RoomComponent>().GetChild<RoomPlayer>(message.PlayerId);
            roomPlayer.IsJoinRoom = true;
            roomComponent.AlreadyJoinRoomCount++;

            if (roomComponent.AlreadyJoinRoomCount <= ConstValue.MatchCount)
            {
                // 通知给已加进来的客户端每个玩家的进度
            }

            if (roomComponent.AlreadyJoinRoomCount == ConstValue.MatchCount)
            {
                await TimerComponent.Instance.WaitAsync(1000);

                Room2C_EnterMap room2CEnterMap = new Room2C_EnterMap() {UnitInfo = new List<LockStepUnitInfo>()};
                foreach (var kv in roomComponent.Children)
                {
                    room2CEnterMap.UnitInfo.Add(new LockStepUnitInfo()
                    {
                        PlayerId = kv.Key, 
                        Position = new TSVector(10, 0, 10), 
                        Rotation = TSQuaternion.identity
                    });
                }

                RoomMessageHelper.BroadCast(room, room2CEnterMap);
            }

            await ETTask.CompletedTask;
        }
    }
}