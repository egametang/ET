using System;
using System.Collections.Generic;

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

        public static async ETTask Match(this MatchComponent self, (long, long) playerInfo)
        {
            if (self.waitMatchPlayers.Contains(playerInfo))
            {
                return;
            }
            
            self.waitMatchPlayers.Add(playerInfo);

            if (self.waitMatchPlayers.Count < ConstValue.MatchCount)
            {
                return;
            }
            
            // 申请一个房间
            StartSceneConfig startSceneConfig = RandomGenerator.RandomArray(StartSceneConfigCategory.Instance.Maps);
            Match2Map_GetRoom match2MapGetRoom = new Match2Map_GetRoom() {PlayerInfo = new Dictionary<long, long>()};
            foreach ((long id, long sessionInstanceId) in self.waitMatchPlayers)
            {
                match2MapGetRoom.PlayerInfo.Add(id, sessionInstanceId);
            }
            self.waitMatchPlayers.Clear();
            
            Map2Match_GetRoom map2MatchGetRoom = await ActorMessageSenderComponent.Instance.Call(
                startSceneConfig.InstanceId, match2MapGetRoom) as Map2Match_GetRoom;
            foreach (var kv in match2MapGetRoom.PlayerInfo) // 这里发送消息线程不会修改PlayerInfo，所以可以直接使用
            {
                ActorMessageSenderComponent.Instance.Send(
                    kv.Value, new Match2G_NotifyMatchSuccess() {InstanceId = map2MatchGetRoom.InstanceId});
                // 等待进入房间的确认消息，如果超时要通知所有玩家退出房间，重新匹配
            }
        }
    }

}