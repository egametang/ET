using System.Collections.Generic;
using System.Diagnostics;

namespace ET.Server
{
    [ComponentOf(typeof(VProcess))]
    public class WatcherComponent: SingletonEntity<WatcherComponent>, IAwake, IDestroy
    {
        public readonly Dictionary<int, System.Diagnostics.Process> Processes = new Dictionary<int, System.Diagnostics.Process>();
    }
}