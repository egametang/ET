using System.Collections;
using System.Diagnostics;

namespace ET.Server
{
    [FriendOf(typeof(WatcherComponent))]
    public static partial class WatcherComponentSystem
    {
        public static void Start(this WatcherComponent self, int createScenes = 0)
        {
            string[] localIP = NetworkHelper.GetAddressIPs();
            var processConfigs = StartProcessConfigCategory.Instance.GetAll();
            foreach (StartProcessConfig startProcessConfig in processConfigs.Values)
            {
                if (!WatcherHelper.IsThisMachine(startProcessConfig.InnerIP, localIP))
                {
                    continue;
                }
                System.Diagnostics.Process process = WatcherHelper.StartProcess(startProcessConfig.Id, createScenes);
                self.Processes.Add(startProcessConfig.Id, process);
            }
        }
    }
}