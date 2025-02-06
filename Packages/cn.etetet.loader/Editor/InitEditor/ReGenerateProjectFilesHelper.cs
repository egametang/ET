using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class ReGenerateProjectFilesHelper
    {
        [InitializeOnLoadMethod]
        [MenuItem("ET/Loader/ReGenerateProjectFiles")]
        public static void Run()
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