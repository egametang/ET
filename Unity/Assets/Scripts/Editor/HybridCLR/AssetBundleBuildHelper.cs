using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor
{
    public static class AssetBundleBuildHelper
    {

        public static string ToRelativeAssetPath(string s)
        {
            return s.Substring(s.IndexOf("Assets/"));
        }

        /// <summary>
        /// 将HotFix.dll和HotUpdatePrefab.prefab打入common包.
        /// 将HotUpdateScene.unity打入scene包.
        /// </summary>
        /// <param name="tempDir"></param>
        /// <param name="outputDir"></param>
        /// <param name="target"></param>
        private static void BuildAssetBundles(string tempDir, string outputDir, BuildTarget target)
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(outputDir);
            CompileDllHelper.CompileDll(target);


            List<string> notSceneAssets = new List<string>();

            string hotfixDllSrcDir = BuildConfig.GetHotFixDllsOutputDirByTarget(target);
            foreach (var dll in BuildConfig.HotUpdateAssemblies)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                string dllBytesPath = $"{tempDir}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                notSceneAssets.Add(dllBytesPath);
            }

            string aotDllDir = BuildConfig.GetAssembliesPostIl2CppStripDir(target);
            foreach (var dll in BuildConfig.AOTMetaAssemblies)
            {
                string dllPath = $"{aotDllDir}/{dll}";
                if (!File.Exists(dllPath))
                {
                    Debug.LogError($"ab中添加AOT补充元数据dll:{dllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }
                string dllBytesPath = $"{tempDir}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                notSceneAssets.Add(dllBytesPath);
            }

            string testPrefab = $"{Application.dataPath}/Prefabs/HotUpdatePrefab.prefab";
            notSceneAssets.Add(testPrefab);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            List<AssetBundleBuild> abs = new List<AssetBundleBuild>();
            AssetBundleBuild notSceneAb = new AssetBundleBuild
            {
                assetBundleName = "common",
                assetNames = notSceneAssets.Select(s => ToRelativeAssetPath(s)).ToArray(),
            };
            abs.Add(notSceneAb);

            UnityEditor.BuildPipeline.BuildAssetBundles(outputDir, abs.ToArray(), BuildAssetBundleOptions.None, target);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            string streamingAssetPathDst = $"{Application.streamingAssetsPath}";
            Directory.CreateDirectory(streamingAssetPathDst);

            foreach (var ab in abs)
            {
                AssetDatabase.CopyAsset(ToRelativeAssetPath($"{outputDir}/{ab.assetBundleName}"),
                    ToRelativeAssetPath($"{streamingAssetPathDst}/{ab.assetBundleName}"));
            }
        }

        public static void BuildAssetBundleByTarget(BuildTarget target)
        {
            BuildAssetBundles(BuildConfig.GetAssetBundleTempDirByTarget(target), BuildConfig.GetAssetBundleOutputDirByTarget(target), target);
        }

        //[MenuItem("HybridCLR/BuildBundles/ActiveBuildTarget")]
        public static void BuildSceneAssetBundleActiveBuildTarget()
        {
            BuildAssetBundleByTarget(EditorUserBuildSettings.activeBuildTarget);
        }

        //[MenuItem("HybridCLR/BuildBundles/Win64")]
        public static void BuildSceneAssetBundleWin64()
        {
            var target = BuildTarget.StandaloneWindows64;
            BuildAssetBundleByTarget(target);
        }

        //[MenuItem("HybridCLR/BuildBundles/Win32")]
        public static void BuildSceneAssetBundleWin32()
        {
            var target = BuildTarget.StandaloneWindows;
            BuildAssetBundleByTarget(target);
        }

        //[MenuItem("HybridCLR/BuildBundles/Android")]
        public static void BuildSceneAssetBundleAndroid()
        {
            var target = BuildTarget.Android;
            BuildAssetBundleByTarget(target);
        }

        //[MenuItem("HybridCLR/BuildBundles/IOS")]
        public static void BuildSceneAssetBundleIOS()
        {
            var target = BuildTarget.iOS;
            BuildAssetBundleByTarget(target);
        }
    }
}
