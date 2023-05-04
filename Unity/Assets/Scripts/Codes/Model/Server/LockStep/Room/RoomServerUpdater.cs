using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Room))]
    public class RoomServerUpdater: Entity, IAwake, IUpdate
    {
    }
}