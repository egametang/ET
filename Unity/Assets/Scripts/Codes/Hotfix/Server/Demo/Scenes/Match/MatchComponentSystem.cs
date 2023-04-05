using System;

namespace ET.Server
{

    [FriendOf(typeof(MatchComponent))]
    public static class MatchComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<MatchComponent>
        {
            protected override void Awake(MatchComponent self)
            {
                
            }
        }

        private const int MatchCount = 1;

        public static async ETTask Match(this MatchComponent self, (long, long) playerInfo)
        {
            if (self.waitMatchPlayers.Contains(playerInfo))
            {
                return;
            }
            
            self.waitMatchPlayers.Add(playerInfo);

            if (self.waitMatchPlayers.Count < MatchCount)
            {
                return;
            }
            
            // 申请一个房间
            StartSceneConfig startSceneConfig = RandomGenerator.RandomArray(StartSceneConfigCategory.Instance.Robots);
            Map2Match_GetRoom map2MatchGetRoom = await ActorMessageSenderComponent.Instance.Call(
                startSceneConfig.InstanceId, new Match2Map_GetRoom()) as Map2Match_GetRoom;

            for (int i = 0; i < MatchCount; ++i)
            {
                ActorMessageSenderComponent.Instance.Send(
                    startSceneConfig.InstanceId, new Match2G_NotifyMatchSuccess() {InstanceId = map2MatchGetRoom.InstanceId});
                // 等待进入房间的确认消息，如果超时要通知所有玩家退出房间，重新匹配
            }
        }
    }

}