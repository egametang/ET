using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(BattleScene))]
    public class RoomServerComponent: Entity, IAwake<Match2Map_GetRoom>
    {
        public int AlreadyJoinRoomCount;
    }
}