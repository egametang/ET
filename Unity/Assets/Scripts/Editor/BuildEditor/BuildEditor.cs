using System.IO;
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

    public class BuildEditor : EditorWindow
    {
        private PlatformType activePlatform;
        private PlatformType platformType;
        private bool clearFolder;
        private BuildOptions buildOptions;
        private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;

        private GlobalConfig globalConfig;

        [MenuItem("ET/Build Tool")]
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
            this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
            this.clearFolder = EditorGUILayout.Toggle("clean folder? ", clearFolder);
            BuildType codeOptimization = (BuildType)EditorGUILayout.EnumPopup("BuildType ", this.globalConfig.BuildType);

            if (codeOptimization != this.globalConfig.BuildType)
            {
                this.globalConfig.BuildType = codeOptimization;
                EditorUtility.SetDirty(this.globalConfig);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.LabelField("BuildAssetBundleOptions ");
            this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);

            switch (this.globalConfig.BuildType)
            {
                case BuildType.None:
                case BuildType.Debug:
                    this.buildOptions = BuildOptions.BuildScriptsOnly;
                    break;
                case BuildType.Release:
                    this.buildOptions = BuildOptions.BuildScriptsOnly;
                    break;
            }

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
                    Log.Error("build package CodeMode must be CodeMode.Client, please select Client, RegenerateCSProject, then rebuild Hotfix and Model !!!");
                    return;
                }

                if (this.globalConfig.EPlayMode == EPlayMode.EditorSimulateMode)
                {
                    Log.Error("build package EPlayMode must not be EPlayMode.EditorSimulateMode, please select EditorMode");
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
                BuildHelper.Build(this.platformType, this.buildAssetBundleOptions, this.buildOptions, this.clearFolder);
                return;
            }

            GUILayout.Label("");
            GUILayout.Label("Code Compile：");
            EditorGUI.BeginChangeCheck();
            CodeMode codeMode = (CodeMode)EditorGUILayout.EnumPopup("CodeMode: ", this.globalConfig.CodeMode);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this.globalConfig);
                AssetDatabase.SaveAssetIfDirty(this.globalConfig);
                AssetDatabase.Refresh();
            }

            if (codeMode != this.globalConfig.CodeMode)
            {
                this.globalConfig.CodeMode = codeMode;
                EditorUtility.SetDirty(this.globalConfig);
                AssetDatabase.SaveAssets();
                
                BuildHelper.ReGenerateProjectFiles();
            }

            EPlayMode ePlayMode = (EPlayMode)EditorGUILayout.EnumPopup("EPlayMode: ", this.globalConfig.EPlayMode);
            if (ePlayMode != this.globalConfig.EPlayMode)
            {
                this.globalConfig.EPlayMode = ePlayMode;
                EditorUtility.SetDirty(this.globalConfig);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("ReGenerateProjectFiles"))
            {
                if (Define.EnableDll)
                {
                    // 若没有生成以下工程，则切换到非ENABLE_DLL模式进行编译，编译完再切换回来, 保证代码编辑器正常显示所有项目
                    if (!File.Exists("./Unity.Hotfix.csproj") || !File.Exists("./Unity.HotfixView.csproj") ||
                        !File.Exists("./Unity.Model.csproj") || !File.Exists("./Unity.ModelView.csproj"))
                    {
                        BuildHelper.EnableDefineSymbols("ENABLE_DLL", false);
                        BuildHelper.EnableDefineSymbols("ENABLE_DLL", true);
                    }
                }

                BuildHelper.ReGenerateProjectFiles();
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
