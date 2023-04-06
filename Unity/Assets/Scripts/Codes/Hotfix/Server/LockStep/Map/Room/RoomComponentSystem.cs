using System;

namespace ET.Server
{
    [FriendOf(typeof(RoomComponent))]
    public static class RoomComponentSystem
    {
        [ObjectSystem]
        public class RoomComponentAwakeSystem: AwakeSystem<RoomComponent, Match2Map_GetRoom>
        {
            protected override void Awake(RoomComponent self, Match2Map_GetRoom match2MapGetRoom)
            {
                foreach (var kv in match2MapGetRoom.PlayerInfo)
                {
                    RoomPlayer roomPlayer = self.AddChildWithId<RoomPlayer>(kv.Key);
                    roomPlayer.SessionInstanceId = kv.Value;
                }
            }
        }
    }
}