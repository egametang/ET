using System;
using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(RoomServerComponent))]
    [FriendOf(typeof(RoomServerComponent))]
    public static partial class RoomServerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this RoomServerComponent self, List<long> playerIds)
        {
            foreach (long id in playerIds)
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