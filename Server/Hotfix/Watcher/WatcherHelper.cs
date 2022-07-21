using ET.StartServer;
using System;
using System.Collections;
using System.Diagnostics;

namespace ET
{
    public static class WatcherHelper
    {
        public static StartMachine GetThisMachineConfig()
        {
            string[] localIP = NetworkHelper.GetAddressIPs();
            StartMachine startMachineConfig = null;
            foreach (StartMachine config in Tables.Ins.TbStartMachine.DataMap.Values)
            {
                if (!WatcherHelper.IsThisMachine(config.InnerIp, localIP))
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
            if (ip != "127.0.0.1" && ip != "0.0.0.0" && !((IList)localIPs).Contains(ip))
            {
                return false;
            }
            return true;
        }

        public static Process StartProcess(int processId, int createScenes = 0)
        {
            StartProcess startProcessConfig = Tables.Ins.TbStartProcess.Get(processId);
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