using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class BuildHelper
    {
        private const string relativeDirPrefix = "../Release";

        public static string BuildFolder = "../Release/{0}/StreamingAssets/";

        [InitializeOnLoadMethod]
        public static void ReGenerateProjectFiles()
        {
            Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
            Debug.Log("ReGenerateProjectFiles finished.");
        }

#if ENABLE_DLL
        [MenuItem("ET/ChangeDefine/Remove ENABLE_DLL")]
        public static void RemoveEnableDll()
        {
            EnableDefineSymbols("ENABLE_DLL", false);
        }
#else
        [MenuItem("ET/ChangeDefine/Add ENABLE_DLL")]
        public static void AddEnableDll()
        {
            EnableDefineSymbols("ENABLE_DLL", true);
        }
#endif

#if ENABLE_VIEW
        [MenuItem("ET/ChangeDefine/Remove ENABLE_VIEW")]
        public static void RemoveEnableView()
        {
            EnableDefineSymbols("ENABLE_VIEW", false);
        }
#else
        [MenuItem("ET/ChangeDefine/Add ENABLE_VIEW")]
        public static void AddEnableView()
        {
            EnableDefineSymbols("ENABLE_VIEW", true);
        }
#endif
        public static void EnableDefineSymbols(string symbols, bool enable)
        {
            Debug.Log($"EnableDefineSymbols {symbols} {enable}");
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var ss = defines.Split(';').ToList();
            if (enable)
            {
                if (ss.Contains(symbols))
                {
                    return;
                }
                ss.Add(symbols);
            }
            else
            {
                if (!ss.Contains(symbols))
                {
                    return;
                }
                ss.Remove(symbols);
            }
            Debug.Log($"EnableDefineSymbols {symbols} {enable}");
            defines = string.Join(";", ss);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void Build(PlatformType type, BuildOptions buildOptions)
        {
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            string programName = "ET";
            string exeName = programName;
            switch (type)
            {
                case PlatformType.Windows:
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
                case PlatformType.Linux:
                    buildTarget = BuildTarget.StandaloneLinux64;
                    break;
            }

            string fold = string.Format(BuildFolder, type);

            Directory.CreateDirectory(fold);

            AssetDatabase.Refresh();
            string[] levels = {
                "Assets/Scenes/Init.unity",
            };
            Debug.Log("start build exe");
            BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
            Debug.Log("finish build exe");
        }
    }
}