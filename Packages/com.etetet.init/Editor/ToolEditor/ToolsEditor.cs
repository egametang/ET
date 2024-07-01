using UnityEditor;

namespace ET
{
    public static class ToolsEditor
    {
        [MenuItem("ET/ExcelExporter")]
        public static void ExcelExporter()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "dotnet";
#else
            const string tools = "dotnet.exe";
#endif
            ShellHelper.Run($"{tools} ./Packages/cn.etetet.excel/DotNet~/Exe/ET.ExcelExporter.dll", "./");
        }
        
        [MenuItem("ET/Proto2CS")]
        public static void Proto2CS()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "dotnet";
#else
            const string tools = "dotnet.exe";
#endif
            ShellHelper.Run($"{tools} ./Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll", "./");
        }
    }
}