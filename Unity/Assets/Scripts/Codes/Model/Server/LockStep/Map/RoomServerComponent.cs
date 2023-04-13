using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class RoomServerComponent: Entity, IAwake<Match2Map_GetRoom>
    {
        public int AlreadyJoinRoomCount;
    }
}