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

        public static bool IsAllPlayerProgress100(this RoomServerComponent self)
        {
            foreach (RoomPlayer roomPlayer in self.Children.Values)
            {
                if (roomPlayer.Progress != 100)
                {
                    return false;
                }
            }
            return true;
        }
    }
}