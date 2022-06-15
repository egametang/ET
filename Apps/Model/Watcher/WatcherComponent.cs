using System.Collections.Generic;
using System.Diagnostics;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class WatcherComponent: Entity, IAwake, IDestroy
    {
        public static WatcherComponent Instance { get; set; }

        public readonly Dictionary<int, Process> Processes = new Dictionary<int, Process>();
    }
}