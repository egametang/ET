using UnityEditor;
using System.Diagnostics;

namespace ET
{
    public static class ToolsEditor
    {
        [MenuItem("ET/ExcelExporter")]
        public static void ExcelExporter()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "/usr/local/bin/dotnet";
#else
            const string tools = "dotnet.exe";
#endif
            Process process = ProcessHelper.Run($"{tools}", "./Packages/cn.etetet.excel/DotNet~/Exe/ET.ExcelExporter.dll", "./", true);

            UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
        }
        
        [MenuItem("ET/Proto2CS")]
        public static void Proto2CS()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "/usr/local/bin/dotnet";
#else
            const string tools = "dotnet.exe";
#endif
            Process process = ProcessHelper.Run($"{tools}", "./Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll", "./", true);

            UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
        }
    }
}