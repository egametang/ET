using System.Collections.Generic;
using TrueSync;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    [FriendOf(typeof (RoomServerComponent))]
    public class C2Room_ChangeSceneFinishHandler: AMActorHandler<Scene, C2Room_ChangeSceneFinish>
    {
        protected override async ETTask Run(Scene roomScene, C2Room_ChangeSceneFinish message)
        {
            RoomServerComponent roomServerComponent = roomScene.GetComponent<RoomServerComponent>();
            RoomPlayer roomPlayer = roomScene.GetComponent<RoomServerComponent>().GetChild<RoomPlayer>(message.PlayerId);
            roomPlayer.IsJoinRoom = true;
            roomServerComponent.AlreadyJoinRoomCount++;

            if (roomServerComponent.AlreadyJoinRoomCount <= LSConstValue.MatchCount)
            {
                // 通知给已加进来的客户端每个玩家的进度
            }

            if (roomServerComponent.AlreadyJoinRoomCount == LSConstValue.MatchCount)
            {
                await TimerComponent.Instance.WaitAsync(1000);

                Room2C_BattleStart room2CBattleStart = new() {StartTime = TimeHelper.ServerFrameTime()};
                foreach (RoomPlayer rp in roomServerComponent.Children.Values)
                {
                    room2CBattleStart.UnitInfo.Add(new LockStepUnitInfo()
                    {
                        PlayerId = rp.Id, 
                        Position = new TSVector(10, 0, 10), 
                        Rotation = TSQuaternion.identity
                    });
                }
                
                roomScene.GetComponent<BattleScene>().Init(room2CBattleStart);

                RoomMessageHelper.BroadCast(roomScene, room2CBattleStart);
            }

            await ETTask.CompletedTask;
        }
    }
}