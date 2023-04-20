using System;

namespace ET.Server
{
    [FriendOf(typeof(RoomServerComponent))]
    public static class RoomServerComponentSystem
    {
        [ObjectSystem]
        public class RoomServerComponentAwakeSystem: AwakeSystem<RoomServerComponent, Match2Map_GetRoom>
        {
            protected override void Awake(RoomServerComponent self, Match2Map_GetRoom match2MapGetRoom)
            {
                int slot = 0;
                foreach (long id in match2MapGetRoom.PlayerIds)
                {
                    RoomPlayer roomPlayer = self.AddChildWithId<RoomPlayer>(id);
                    roomPlayer.Slot = slot++;
                }
            }
        }
    }
}