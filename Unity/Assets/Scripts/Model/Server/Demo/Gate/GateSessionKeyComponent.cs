using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Fiber))]
    public class GateSessionKeyComponent : Entity, IAwake
    {
        public readonly Dictionary<long, string> sessionKey = new();
    }
}