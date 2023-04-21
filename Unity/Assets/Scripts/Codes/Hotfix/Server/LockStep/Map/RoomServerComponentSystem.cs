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
                foreach (long id in match2MapGetRoom.PlayerIds)
                {
                    RoomPlayer roomPlayer = self.AddChildWithId<RoomPlayer>(id);
                }
            }
        }
    }
}