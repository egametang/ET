using System.Collections;
using System.Diagnostics;

namespace ET.Server
{
    [EntitySystemOf(typeof(WatcherComponent))]
    public static partial class WatcherComponentSystem
    {
        [EntitySystem]
        public static void Awake(this WatcherComponent self)
        {
            string[] localIP = NetworkHelper.GetAddressIPs();
            var processConfigs = StartProcessConfigCategory.Instance.GetAll();
            foreach (StartProcessConfig startProcessConfig in processConfigs.Values)
            {
                if (!WatcherHelper.IsThisMachine(startProcessConfig.InnerIP, localIP))
                {
                    continue;
                }
                System.Diagnostics.Process process = WatcherHelper.StartProcess(startProcessConfig.Id);
                self.Processes.Add(startProcessConfig.Id, process);
            }
        }
    }
}