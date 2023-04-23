using System.Collections.Generic;
using TrueSync;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Battle)]
    [FriendOf(typeof (RoomServerComponent))]
    public class C2Room_ChangeSceneFinishHandler: AMActorHandler<BattleScene, C2Battle_ChangeSceneFinish>
    {
        protected override async ETTask Run(BattleScene battleScene, C2Battle_ChangeSceneFinish message)
        {
            RoomServerComponent roomServerComponent = battleScene.GetComponent<RoomServerComponent>();
            RoomPlayer roomPlayer = battleScene.GetComponent<RoomServerComponent>().GetChild<RoomPlayer>(message.PlayerId);
            roomPlayer.IsJoinRoom = true;
            roomServerComponent.AlreadyJoinRoomCount++;

            if (roomServerComponent.AlreadyJoinRoomCount <= LSConstValue.MatchCount)
            {
                // 通知给已加进来的客户端每个玩家的进度
            }

            if (roomServerComponent.AlreadyJoinRoomCount == LSConstValue.MatchCount)
            {
                await TimerComponent.Instance.WaitAsync(1000);

                Battle2C_BattleStart room2CBattleStart = new() {StartTime = TimeHelper.ServerFrameTime()};
                foreach (RoomPlayer rp in roomServerComponent.Children.Values)
                {
                    room2CBattleStart.UnitInfo.Add(new LockStepUnitInfo()
                    {
                        PlayerId = rp.Id, 
                        Position = new TSVector(10, 0, 10), 
                        Rotation = TSQuaternion.identity
                    });
                }
                
                battleScene.Init(room2CBattleStart);

                RoomMessageHelper.BroadCast(battleScene, room2CBattleStart);
            }

            await ETTask.CompletedTask;
        }
    }
}