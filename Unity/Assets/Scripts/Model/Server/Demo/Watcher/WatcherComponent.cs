using System.Collections.Generic;
using System.Diagnostics;

namespace ET.Server
{
    [ComponentOf(typeof(RootEntity))]
    public class WatcherComponent: Entity, IAwake, IDestroy
    {
        public static WatcherComponent Instance { get; set; }

        public readonly Dictionary<int, System.Diagnostics.Process> Processes = new Dictionary<int, System.Diagnostics.Process>();
    }
}