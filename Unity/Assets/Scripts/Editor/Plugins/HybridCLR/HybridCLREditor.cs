using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using UnityEditor;

namespace ET
{
    public static class HybridCLREditor
    {
        private static readonly List<string> AotDlls = new()
        {
            "Unity.ThirdParty.dll", 
            "Unity.Loader.dll", 
            "Unity.Core.dll", 
            "MongoDB.Bson.dll",
            "CommandLine.dll",
            "NLog.dll",
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
        
        [MenuItem("HybridCLR/CopyAotDlls")]
        public static void CopyAotDll()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string fromDir = Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());
            string toDir = "Assets/Bundles/AotDlls";
            if (!Directory.Exists(toDir))
            {
                Directory.CreateDirectory(toDir);
            }
            AssetDatabase.Refresh();
            
            foreach (string aotDll in AotDlls)
            {
                File.Copy(Path.Combine(fromDir, aotDll), Path.Combine(toDir, $"{aotDll}.bytes"), true);
            }
            
            // 设置ab包
            AssetImporter assetImporter = AssetImporter.GetAtPath(toDir);
            assetImporter.assetBundleName = "AotDlls.unity3d";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}