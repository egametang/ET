using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ET
{
    public static class BuildHelper
    {
        private const string relativeDirPrefix = "../Release";

        public static string BuildFolder = "../Release/{0}/StreamingAssets/";

        
#if ENABLE_CODES
        [MenuItem("ET/ChangeDefine/Remove ENABLE_CODES")]
#else
        [MenuItem("ET/ChangeDefine/Add ENABLE_CODES")]
#endif
        public static void EnableCodes()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var ss = defines.Split(';').ToList();
#if ENABLE_CODES
            if (!ss.Contains("ENABLE_CODES"))
            {
                return;
            }
            ss.Remove("ENABLE_CODES");
#else
            if (ss.Contains("ENABLE_CODES"))
            {
                return;
            }
            ss.Add("ENABLE_CODES");
#endif
            defines = string.Join(";", ss);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
            AssetDatabase.SaveAssets();
        }

        public static void Build(PlatformType type, BuildAssetBundleOptions buildAssetBundleOptions, BuildOptions buildOptions, bool isBuildExe, bool isContainAB, bool clearFolder)
        {
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            string programName = "ET";
            string exeName = programName;
            switch (type)
            {
                case PlatformType.PC:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    break;
                case PlatformType.Android:
                    buildTarget = BuildTarget.Android;
                    exeName += ".apk";
                    break;
                case PlatformType.IOS:
                    buildTarget = BuildTarget.iOS;
                    break;
                case PlatformType.MacOS:
                    buildTarget = BuildTarget.StandaloneOSX;
                    break;
            }

            string fold = string.Format(BuildFolder, type);

            if (clearFolder && Directory.Exists(fold))
            {
                Directory.Delete(fold, true);
            }
            Directory.CreateDirectory(fold);

            UnityEngine.Debug.Log("start build assetbundle");
            BuildPipeline.BuildAssetBundles(fold, buildAssetBundleOptions, buildTarget);

            UnityEngine.Debug.Log("finish build assetbundle");

            if (isContainAB)
            {
                FileHelper.CleanDirectory("Assets/StreamingAssets/");
                FileHelper.CopyDirectory(fold, "Assets/StreamingAssets/");
            }

            if (isBuildExe)
            {
                AssetDatabase.Refresh();
                string[] levels = {
                    "Assets/Scenes/Init.unity",
                };
                UnityEngine.Debug.Log("start build exe");
                BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
                UnityEngine.Debug.Log("finish build exe");
            }
            else
            {
                if (isContainAB && type == PlatformType.PC)
                {
                    string targetPath = Path.Combine(relativeDirPrefix, $"{programName}_Data/StreamingAssets/");
                    FileHelper.CleanDirectory(targetPath);
                    Debug.Log($"src dir: {fold}    target: {targetPath}");
                    FileHelper.CopyDirectory(fold, targetPath);
                }
            }
        }
    }
}
