using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Room))]
    public class LSServerUpdater: Entity, IAwake, IUpdate
    {
    }
}