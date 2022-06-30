// --------------------------
// 作者：烟雨迷离半世殇
// 邮箱：1778139321@qq.com
// 日期：2022年6月29日, 星期三
// --------------------------

using System;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace ET
{
    public static class BuildStandalonePlayer
    {
        private const string c_RelativeDirPrefix = "../Release/";
        private const string c_InitScenePath = "Assets/Init.unity";

        [MenuItem("Tools/打包")]
        public static void Build()
        {
            var outputPath =
                $"{c_RelativeDirPrefix}/ProjectS_EXE"; //EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if (outputPath.Length == 0)
                return;

            #region 将Unity的BuildInScene设置为仅包含Init，因为我们为了支持在编辑器模式下的测试而必须将所有Scene放到Unity的BuildInSetting里

            var backScenes = EditorBuildSettings.scenes;
            var scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(c_InitScenePath, true) };
            EditorBuildSettings.scenes = scenes;

            #endregion

            // 如果执行打包，就强行替换为非本地调试模式，进行AB加载
            Init updater =UnityEngine.Object.FindObjectOfType<Init>();
            YooAssets.EPlayMode backPlayMode = updater.PlayMode;
            updater.PlayMode = YooAssets.EPlayMode.HostPlayMode;

            var targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if (targetName == null)
                return;

            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = outputPath + targetName,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);

            updater.PlayMode = backPlayMode;
            EditorBuildSettings.scenes = backScenes;
        }
        
        private static string GetBuildTargetName (BuildTarget target)
        {
            var time = DateTime.Now.ToString ("yyyyMMdd-HHmmss");
            var name = PlayerSettings.productName + "-v" + PlayerSettings.bundleVersion + ".";
            switch (target) {
                case BuildTarget.Android:
                    return string.Format ("/{0}{1}-{2}.apk", name, 1, time);

                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return string.Format ("/{0}{1}-{2}.exe", name, 1, time);

#if UNITY_2017_3_OR_NEWER
                case BuildTarget.StandaloneOSX:
                    return "/" + name + ".app";

#else
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "/" + path + ".app";

#endif

                case BuildTarget.WebGL:
                case BuildTarget.iOS:
                    return "";
                // Add more build targets for your own.
                default:
                    Debug.Log ("Target not implemented.");
                    return null;
            }
        }
    }
}