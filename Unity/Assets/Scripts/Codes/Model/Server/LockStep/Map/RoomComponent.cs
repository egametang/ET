using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class RoomComponent: Entity, IAwake<Match2Map_GetRoom>
    {
        public int AlreadyJoinRoomCount;
    }
}