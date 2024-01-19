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
        Linux
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

        [MenuItem("ET/Build Tool", false, ETMenuItemPriority.BuildTool)]
        public static void ShowWindow()
        {
            GetWindow<BuildEditor>(DockDefine.Types);
        }

        private void OnEnable()
        {
            globalConfig = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Resources/GlobalConfig.asset");

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

                if (this.globalConfig.EPlayMode == EPlayMode.EditorSimulateMode)
                {
                    Log.Error("build package EPlayMode must not be EPlayMode.EditorSimulateMode, please select HostPlayMode");
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

            if (GUILayout.Button("ExcelExporter"))
            {
                ToolsEditor.ExcelExporter();
                return;
            }

            if (GUILayout.Button("Proto2CS"))
            {
                ToolsEditor.Proto2CS();
                return;
            }

            GUILayout.Space(5);
        }
    }
}