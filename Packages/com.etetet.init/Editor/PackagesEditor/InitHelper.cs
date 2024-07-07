using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class InitHelper
    {
        [InitializeOnLoadMethod]
        [MenuItem("ET/ReGenerateProjectFiles")]
        public static void ReGenerateProjectFiles()
        {
            Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();

            foreach (string s in FileHelper.GetAllFiles(".", "Ignore.ET*.csproj"))
            {
                System.IO.File.Delete(s);
            }

            Debug.Log("regenerate csproj");
        }

        [MenuItem("ET/Init")]
        public static void Init()
        {
            LinkSlnHelper.Run();
            
            ToolsEditor.ExcelExporter();
            
            ToolsEditor.Proto2CS();
            
            // 刷新4个程序集的asmdef引用
            ScriptsReferencesHelper.Run();
            
            GlobalConfig globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Packages/com.etetet.init/Resources/GlobalConfig.asset");
            CodeModeChangeHelper.ChangeToCodeMode(globalConfig.CodeMode);
            
            // 设置GlobalConfig中的SceneName字段
            SceneNameSetHelper.Run();
            
            InitScriptHelper.Run();
            
            DefineHelper.EnableDefineSymbols("INITED", true);
            
            AssetDatabase.Refresh();
            
            Debug.Log("Init finish!");
        }

        [MenuItem("ET/Refresh")]
        public static void Refresh()
        {
            ScriptsReferencesHelper.Run();
            
            GlobalConfig globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Packages/com.etetet.init/Resources/GlobalConfig.asset");
            CodeModeChangeHelper.ChangeToCodeMode(globalConfig.CodeMode);
            
            AssetDatabase.Refresh();
            Debug.Log("packages refresh finish!");
        }
    }
}