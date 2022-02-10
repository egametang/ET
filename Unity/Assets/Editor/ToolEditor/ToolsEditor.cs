using UnityEditor;

namespace ET
{
    public static class ToolsEditor
    {
        [MenuItem("Tools/ExcelExporter")]
        public static void ExcelExporter()
        {
#if UNITY_EDITOR_OSX
            const string tools = "./Tools";
#else
            const string tools = ".\\Tools.exe";
#endif
            ShellHelper.Run($"{tools} --AppType=ExcelExporter --Console=1", "../Bin/");
        }
        
        [MenuItem("Tools/Proto2CS")]
        public static void Proto2CS()
        {
#if UNITY_EDITOR_OSX
            const string tools = "./Tools";
#else
            const string tools = ".\\Tools.exe";
#endif
            ShellHelper.Run($"{tools} --AppType=Proto2CS --Console=1", "../Bin/");
        }
    }
}