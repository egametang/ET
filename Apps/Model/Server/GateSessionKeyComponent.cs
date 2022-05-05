using System.Collections.Generic;

namespace ET.Server
{
    public class GateSessionKeyComponent : Entity, IAwake
    {
        public readonly Dictionary<long, string> sessionKey = new Dictionary<long, string>();
    }
}