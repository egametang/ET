using System;

namespace ET.Server
{
    [FriendOf(typeof(RoomServerComponent))]
    public static partial class RoomServerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RoomServerComponent self, Match2Map_GetRoom match2MapGetRoom)
        {
            foreach (long id in match2MapGetRoom.PlayerIds)
            {
                RoomPlayer roomPlayer = self.AddChildWithId<RoomPlayer>(id);
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