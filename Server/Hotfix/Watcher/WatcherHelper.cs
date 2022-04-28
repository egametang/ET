using System;
using System.Collections;
using System.Diagnostics;

namespace ET
{
    public static class WatcherHelper
    {
        public static StartMachineConfig GetThisMachineConfig()
        {
            string[] localIP = NetworkHelper.GetAddressIPs();
            StartMachineConfig startMachineConfig = null;
            foreach (StartMachineConfig config in StartMachineConfigCategory.Instance.GetAll().Values)
            {
                if (!WatcherHelper.IsThisMachine(config.InnerIP, localIP))
                {
                    continue;
                }
                startMachineConfig = config;
                break;
            }

            if (startMachineConfig == null)
            {
                throw new Exception("not found this machine ip config!");
            }

            return startMachineConfig;
        }
        
        public static bool IsThisMachine(string ip, string[] localIPs)
        {
            if (ip != "127.0.0.1" && ip != "0.0.0.0" && !((IList) localIPs).Contains(ip))
            {
                return false;
            }
            return true;
        }
        
        public static Process StartProcess(int processId, int createScenes = 0)
        {
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(processId);
            const string exe = "dotnet";
            string arguments = $"{startProcessConfig.AppName}.dll" + 
                    $" --Process={startProcessConfig.Id}" +
                    $" --AppType={startProcessConfig.AppName}" +
                    $" --StartConfig={Game.Options.StartConfig}" +
                    $" --Develop={Game.Options.Develop}" +
                    $" --CreateScenes={createScenes}" +
                    $" --LogLevel={Game.Options.LogLevel}";
            Log.Debug($"{exe} {arguments}");
            Process process = ProcessHelper.Run(exe, arguments);
            return process;
        }
    }
}