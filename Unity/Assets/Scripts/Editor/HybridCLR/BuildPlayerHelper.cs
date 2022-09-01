using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor
{
    public class BuildPlayerHelper
    {
        public static void CopyAssetBundles(string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            foreach(var ab in BuildConfig.AssetBundleFiles)
            {
                string srcFile = $"{Application.streamingAssetsPath}/{ab}";
                string dstFile = $"{outputDir}/{Path.GetFileName(ab)}";
                File.Copy(srcFile, dstFile, true);
            }
        }

        //[MenuItem("HybridCLR/Build/Win64")]
        public static void Build_Win64()
        {
            BuildTarget target = BuildTarget.StandaloneWindows64;
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            if (activeTarget != BuildTarget.StandaloneWindows64 && activeTarget != BuildTarget.StandaloneWindows)
            {
                Debug.LogError("请先切到Win平台再打包");
                return;
            }
            // Get filename.
            string outputPath = $"{BuildConfig.ProjectDir}/Release-Win64";

            var buildOptions = BuildOptions.None;

            string location = $"{outputPath}/HybridCLRTrial.exe";

            Debug.Log("====> Build App");
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/Init.unity" },
                locationPathName = location,
                options = buildOptions,
                target = target,
                targetGroup = BuildTargetGroup.Standalone,
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError("打包失败");
                return;
            }

            Debug.Log("====> Build AssetBundle");
            AssetBundleBuildHelper.BuildAssetBundleByTarget(target);
            Debug.Log("====> 复制 AssetBundle");
            CopyAssetBundles($"{outputPath}/HybridCLRTrial_Data/StreamingAssets");

#if UNITY_EDITOR
            Application.OpenURL($"file:///{location}");
#endif
        }

        //[MenuItem("HybridCLR/Build/Win32")]
        public static void Build_Win32()
        {
            BuildTarget target = BuildTarget.StandaloneWindows;
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            if (activeTarget != BuildTarget.StandaloneWindows64 && activeTarget != BuildTarget.StandaloneWindows)
            {
                Debug.LogError("请先切到Win平台再打包");
                return;
            }
            // Get filename.
            string outputPath = $"{BuildConfig.ProjectDir}/Release-Win32";

            var buildOptions = BuildOptions.None;

            string location = $"{outputPath}/HybridCLRTrial.exe";

            Debug.Log("====> Build App");
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/main.unity" },
                locationPathName = location,
                options = buildOptions,
                target = target,
                targetGroup = BuildTargetGroup.Standalone,
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError("打包失败");
                return;
            }

            Debug.Log("====> Build AssetBundle");
            AssetBundleBuildHelper.BuildAssetBundleByTarget(target);
            Debug.Log("====> 复制 AssetBundle");
            CopyAssetBundles($"{outputPath}/HybridCLRTrial_Data/StreamingAssets");

#if UNITY_EDITOR
            Application.OpenURL($"file:///{outputPath}");
#endif
        }
        
        //[MenuItem("HybridCLR/Build/OSX")]
        public static void Build_OSX()
        {
            BuildTarget target = BuildTarget.StandaloneWindows;
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            if (activeTarget != BuildTarget.StandaloneOSX)
            {
                Debug.LogError("请先切到Mac平台再打包");
                return;
            }
            // Get filename.
            string outputPath = $"{BuildConfig.ProjectDir}/Release-OSX";

            var buildOptions = BuildOptions.None;

            string location = $"{outputPath}/HybridCLRTrial.app";

            Debug.Log("====> Build App");
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/main.unity" },
                locationPathName = location,
                options = buildOptions,
                target = target,
                targetGroup = BuildTargetGroup.Standalone,
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError("打包失败");
                return;
            }

            Debug.Log("====> Build AssetBundle");
            AssetBundleBuildHelper.BuildAssetBundleByTarget(target);
            Debug.Log("====> 复制 AssetBundle");
            CopyAssetBundles($"{outputPath}/HybridCLRTrial_Data/StreamingAssets");

#if UNITY_EDITOR
            Application.OpenURL($"file:///{outputPath}");
#endif
        }

        //[MenuItem("HybridCLR/Build/Android64")]
        public static void Build_Android64()
        {
            BuildTarget target = BuildTarget.Android;
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            if (activeTarget != BuildTarget.Android)
            {
                Debug.LogError("请先切到Android平台再打包");
                return;
            }
            // Get filename.
            string outputPath = $"{BuildConfig.ProjectDir}/Release-Android";

            var buildOptions = BuildOptions.None;

            string location = outputPath + "/HybridCLRTrial.apk";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/Scenes/main.unity" },
                locationPathName = location,
                options = buildOptions,
                target = target,
                targetGroup = BuildTargetGroup.Android,
            };

            Debug.Log("====> 第1次 Build App(为了生成补充AOT元数据dll)");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("====> Build AssetBundle");
            AssetBundleBuildHelper.BuildAssetBundleByTarget(target);

            Debug.Log("====> 第2次打包");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
#if UNITY_EDITOR
            Application.OpenURL($"file:///{outputPath}");
#endif
        }
    }
}
