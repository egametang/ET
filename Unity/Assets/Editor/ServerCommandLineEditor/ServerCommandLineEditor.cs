using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class ServerCommandLineEditor
    {
        [MenuItem("Tools/启动单进程服务器(仅windows可用)")]
        public static void ShowWindow()
        {
            string arguments = $"Server.dll --Process=1 --Console=1";
            ProcessHelper.Run("dotnet.exe", arguments, "../Bin/");
        }
    }
}