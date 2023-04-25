using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Room))]
    public class RoomServerUpdater: Entity, IAwake, IUpdate
    {
        public int NowFrame { get; set; }
        public Dictionary<long, OneFrameMessages> FrameMessages = new ();
    }
}