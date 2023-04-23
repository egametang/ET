using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Room))]
    public class ServerFrameRecvComponent: Entity, IAwake
    {
        public int NowFrame;
        public Dictionary<long, OneFrameMessages> FrameMessages = new ();
    }
}