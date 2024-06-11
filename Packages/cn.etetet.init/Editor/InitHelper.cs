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
    }
}