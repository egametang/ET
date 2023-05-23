using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    public static class PortHelper
    {
        /// <summary>
	/// 初始化ET时调用1次即可
        /// 清理Unity的残留进程, 解除残留进程对端口的占用
        /// </summary>
        public static void ClearUnityProcess()
        {
            if (Application.isEditor)
            {
                Process[] allProcess = Process.GetProcesses();

                List<Process> unityProcesses = new List<Process>();
                foreach (var process in allProcess)
                {
                    if (!process.HasExited && process.ProcessName == "Unity")
                    {
                        unityProcesses.Add(process);
                    }
                }

                if (unityProcesses.Count > 1)
                {
                    //kill 掉后启动的所有process 对多开的unity会造成影响, 不过ET7本身多开就不方便
                    unityProcesses.Sort((a, b) => a.StartTime < b.StartTime ? 0 : 1);

                    for (int i = 1; i < unityProcesses.Count; i++)
                    {
                        var process = unityProcesses[i];
                        process.Kill();
                        //Log.Info($"Kill Process {process.ProcessName}  PID:" + process.Id);
                    }
                }
            }
        }
    }
}