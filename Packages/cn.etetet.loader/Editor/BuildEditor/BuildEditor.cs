using UnityEditor;
using UnityEngine;
using YooAsset;

namespace ET
{
    public enum PlatformType
    {
        None,
        Android,
        IOS,
        Windows,
        MacOS,
        Linux,
        WebGL
    }

    /// <summary>
    /// ET菜单顺序
    /// </summary>
    public static class ETMenuItemPriority
    {
        public const int BuildTool = 1001;
        public const int ChangeDefine = 1002;
        public const int Compile = 1003;
        public const int Reload = 1004;
        public const int NavMesh = 1005;
        public const int ServerTools = 1006;
    }

    public class BuildEditor : EditorWindow
    {
        private PlatformType activePlatform;
        private PlatformType platformType;
        private BuildOptions buildOptions;

        private GlobalConfig globalConfig;
        private YooConfig yooConfig;

        [MenuItem("ET/Loader/Build Tool", false, ETMenuItemPriority.BuildTool)]
        public static void ShowWindow()
        {
            GetWindow<BuildEditor>();
        }

        private void OnEnable()
        {
            globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Packages/cn.etetet.loader/Resources/GlobalConfig.asset");
            yooConfig = AssetDatabase.LoadAssetAtPath<YooConfig>("Packages/cn.etetet.yooassets/YooConfig.asset");

#if UNITY_ANDROID
            activePlatform = PlatformType.Android;
#elif UNITY_IOS
            activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
            activePlatform = PlatformType.Windows;
#elif UNITY_STANDALONE_OSX
            activePlatform = PlatformType.MacOS;
#elif UNITY_STANDALONE_LINUX
            activePlatform = PlatformType.Linux;
#elif UNITY_WEBGL
            activePlatform = PlatformType.WebGL;
#else
            activePlatform = PlatformType.None;
#endif
            platformType = activePlatform;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("PlatformType ");
            this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);

            EditorGUILayout.LabelField("BuildOptions ");
            this.buildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField(this.buildOptions);

            GUILayout.Space(5);

            if (GUILayout.Button("BuildPackage"))
            {
                if (this.platformType == PlatformType.None)
                {
                    Log.Error("please select platform!");
                    return;
                }

                if (this.globalConfig.CodeMode != CodeMode.Client)
                {
                    Log.Error("build package CodeMode must be CodeMode.Client, please select Client");
                    return;
                }

                if (platformType != activePlatform)
                {
                    switch (EditorUtility.DisplayDialogComplex("Warning!", $"current platform is {activePlatform}, if change to {platformType}, may be take a long time", "change", "cancel", "no change"))
                    {
                        case 0:
                            activePlatform = platformType;
                            break;
                        case 1:
                            return;
                        case 2:
                            platformType = activePlatform;
                            break;
                    }
                }

                BuildHelper.Build(this.platformType, this.buildOptions);
                return;
            }

            GUILayout.Space(5);
        }
    }
}