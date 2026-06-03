using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
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
        }

#if ENABLE_VIEW
        [MenuItem("ET/ChangeDefine/Remove ENABLE_VIEW", false, ETMenuItemPriority.ChangeDefine)]
        public static void RemoveEnableView()
        {
            EnableDefineSymbols("ENABLE_VIEW", false);
        }
#else
        [MenuItem("ET/ChangeDefine/Add ENABLE_VIEW", false, ETMenuItemPriority.ChangeDefine)]
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

            CopyYooAssetBundlesToStreamingAssets(buildTarget);

            AssetDatabase.Refresh();

            Debug.Log("start build exe");

            string[] levels = { "Assets/Scenes/Init.unity" };
            BuildReport report = BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.Log($"BuildResult:{report.summary.result}");
                return;
            }

            Debug.Log("finish build exe");
            EditorUtility.OpenWithDefaultApp(relativeDirPrefix);
        }

        /// <summary>
        /// Copy the latest YooAsset bundles to Assets/StreamingAssets so they are included in the player build.
        /// </summary>
        private static void CopyYooAssetBundlesToStreamingAssets(BuildTarget buildTarget)
        {
            // Read DefaultYooFolderName from YooAssetSettings.asset via SerializedObject
            // (YooAssetSettings class is internal, so we can't access it directly across assemblies)
            string defaultYooFolderName = "yoo"; // fallback default
            string settingsAssetPath = "Assets/Resources/YooAssetSettings.asset";
            UnityEngine.Object settingsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(settingsAssetPath);
            if (settingsAsset != null)
            {
                SerializedObject so = new SerializedObject(settingsAsset);
                SerializedProperty prop = so.FindProperty("DefaultYooFolderName");
                if (prop != null)
                {
                    defaultYooFolderName = prop.stringValue;
                }
            }

            // Map BuildTarget to the directory name YooAsset uses under Bundles/
            string targetDirName = buildTarget switch
            {
                BuildTarget.StandaloneWindows => "StandaloneWindows",
                BuildTarget.StandaloneWindows64 => "StandaloneWindows64",
                BuildTarget.StandaloneOSX => "StandaloneOSX",
                BuildTarget.StandaloneLinux64 => "StandaloneLinux64",
                BuildTarget.Android => "Android",
                BuildTarget.iOS => "iOS",
                _ => buildTarget.ToString()
            };

            string bundlesRoot = Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).FullName, "Bundles", targetDirName);
            if (!Directory.Exists(bundlesRoot))
            {
                Debug.LogWarning($"[BuildHelper] Bundles directory not found: {bundlesRoot}. Skipping copy to StreamingAssets.");
                return;
            }

            // Process each package directory
            foreach (string packageDir in Directory.GetDirectories(bundlesRoot))
            {
                string packageName = Path.GetFileName(packageDir);

                // Find the latest version directory (YooAsset uses date-time format like 2026-05-20-1110, lexicographic = chronological)
                string[] versionDirs = Directory.GetDirectories(packageDir)
                    .OrderByDescending(d => Path.GetFileName(d))
                    .ToArray();
                if (versionDirs.Length == 0)
                {
                    Debug.LogWarning($"[BuildHelper] No version directories found in {packageDir}. Skipping.");
                    continue;
                }

                string latestVersionDir = versionDirs[0];
                string targetPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", defaultYooFolderName, packageName);

                // Clean target directory to avoid stale files from previous builds
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);
                }
                Directory.CreateDirectory(targetPath);

                // Copy files, skipping build-time-only artifacts
                string[] skipPrefixes = { "BuildReport_", "OutputCache" };
                string[] skipExtensions = { ".json" };
                int copiedCount = 0;

                foreach (string srcFile in Directory.GetFiles(latestVersionDir))
                {
                    string fileName = Path.GetFileName(srcFile);

                    if (skipPrefixes.Any(p => fileName.StartsWith(p)))
                        continue;
                    if (skipExtensions.Any(e => fileName.EndsWith(e, System.StringComparison.OrdinalIgnoreCase)))
                        continue;

                    File.Copy(srcFile, Path.Combine(targetPath, fileName));
                    copiedCount++;
                }

                Debug.Log($"[BuildHelper] Copied {copiedCount} files from {latestVersionDir} to {targetPath}");
            }
        }
    }
}