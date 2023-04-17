using System.Collections.Generic;

namespace ET
{
    public class ServerFrameRecvComponent: Entity
    {
        public Dictionary<long, FrameMessage> FrameMessages = new Dictionary<long, FrameMessage>();
    }
}