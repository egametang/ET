using System.Collections.Generic;
using System.Diagnostics;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class WatcherComponent: Entity, IAwake, IDestroy
    {
        public readonly Dictionary<int, System.Diagnostics.Process> Processes = new Dictionary<int, System.Diagnostics.Process>();
    }
}