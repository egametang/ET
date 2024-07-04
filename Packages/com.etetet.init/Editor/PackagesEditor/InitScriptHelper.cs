using System.Diagnostics;
using System.IO;

namespace ET
{
    public static class InitScriptHelper
    {
        public static void Run()
        {
            foreach (string directory in Directory.GetDirectories("Packages", "cn.etetet.*"))
            {
                string initScriptPath = Path.Combine(directory, "Init.ps1");

                UnityEngine.Debug.Log($"run init script start: {initScriptPath}");
                Process process = ProcessHelper.PowerShell($"-NoExit -ExecutionPolicy Bypass -File {initScriptPath}", waitExit: true);
                UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
                UnityEngine.Debug.Log($"run init script finish: {initScriptPath}");
            }
        }
    }
}