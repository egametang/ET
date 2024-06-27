using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class InitHelper
    {
        [InitializeOnLoadMethod]
        public static void ReGenerateProjectFiles()
        {
            Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();

            foreach (string s in FileHelper.GetAllFiles(".", "Ignore.ET*.csproj"))
            {
                System.IO.File.Delete(s);
            }

            Debug.Log("regenerate csproj");
        }

        [MenuItem("ET/Packages Refresh")]
        public static void Refresh()
        {
            AsmdefEditor.UpdateAssemblyDefinition();
            
            GlobalConfig globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Packages/com.etetet.init/Resources/GlobalConfig.asset");
            CodeModeChangeHelper.ChangeToCodeMode(globalConfig.CodeMode);
            
            AssetDatabase.Refresh();
            Debug.Log("packages refresh finish!");
        }
    }
}