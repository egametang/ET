using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ET
{
    public class BundleInfo
	{
		public List<string> ParentPaths = new List<string>();
	}

	public enum PlatformType
	{
		None,
		Android,
		IOS,
		PC,
		MacOS,
	}
	
	public enum BuildType
	{
		Development,
		Release,
	}

	public class BuildEditor : EditorWindow
	{
        private const string settingAsset = "Assets/Editor/BuildEditor/ETBuildSettings.asset";

        private readonly Dictionary<string, BundleInfo> dictionary = new Dictionary<string, BundleInfo>();

		private PlatformType activePlatform;
		private PlatformType platformType;
		private bool clearFolder;
		private bool isBuildExe;
		private bool isContainAB;
		private BuildType buildType;
		private BuildOptions buildOptions;
		private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;

		private ETBuildSettings buildSettings;

		[MenuItem("Tools/打包工具")]
		public static void ShowWindow()
		{
			EditorWindow window = GetWindow<BuildEditor>(true, "打包工具");
			window.minSize = new Vector2(420, 220);
			window.maxSize = new Vector2(700, 400);
		}

        private void OnEnable()
        {
#if UNITY_ANDROID
			activePlatform = PlatformType.Android;
#elif UNITY_IOS
			activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
			activePlatform = PlatformType.PC;
#elif UNITY_STANDALONE_OSX
			activePlatform = PlatformType.MacOS;
#else
			activePlatform = PlatformType.None;
#endif
            platformType = activePlatform;

			if (!File.Exists(settingAsset))
            {
				buildSettings = new ETBuildSettings();
				AssetDatabase.CreateAsset(buildSettings, settingAsset);
            }
			else
			{
				buildSettings = AssetDatabase.LoadAssetAtPath<ETBuildSettings>(settingAsset);

				clearFolder = buildSettings.clearFolder;
				isBuildExe = buildSettings.isBuildExe;
				isContainAB = buildSettings.isContainAB;
				buildType = buildSettings.buildType;
				buildAssetBundleOptions = buildSettings.buildAssetBundleOptions;
			}
        }

        private void OnDisable()
        {
			SaveSettings();
        }

        private void OnGUI() 
		{
			EditorGUILayout.LabelField("打包平台:");
			this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
			this.clearFolder = EditorGUILayout.Toggle("清理资源文件夹: ", clearFolder);
			this.isBuildExe = EditorGUILayout.Toggle("是否打包EXE: ", this.isBuildExe);
			this.isContainAB = EditorGUILayout.Toggle("是否同将资源打进EXE: ", this.isContainAB);
			this.buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType: ", this.buildType);
			EditorGUILayout.LabelField("BuildAssetBundleOptions(可多选):");
			this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);
			
			switch (buildType)
			{
				case BuildType.Development:
					this.buildOptions = BuildOptions.Development | BuildOptions.AutoRunPlayer | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
					break;
				case BuildType.Release:
					this.buildOptions = BuildOptions.None;
					break;
			}

			GUILayout.Space(5);

			if (GUILayout.Button("开始打包", GUILayout.ExpandHeight(true)))
			{
				if (this.platformType == PlatformType.None)
				{
					ShowNotification(new GUIContent("请选择打包平台!"));
					return;
				}
				if (platformType != activePlatform)
                {
                    switch (EditorUtility.DisplayDialogComplex("警告!", $"当前目标平台为{activePlatform}, 如果切换到{platformType}, 可能需要较长加载时间", "切换", "取消", "不切换"))
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
				BuildHelper.Build(this.platformType, this.buildAssetBundleOptions, this.buildOptions, this.isBuildExe, this.isContainAB, this.clearFolder);
			}

			GUILayout.Space(5);
		}

		private void SaveSettings()
        {
			buildSettings.clearFolder = clearFolder;
			buildSettings.isBuildExe = isBuildExe;
			buildSettings.isContainAB = isContainAB;
			buildSettings.buildType = buildType;
			buildSettings.buildAssetBundleOptions = buildAssetBundleOptions;

			EditorUtility.SetDirty(buildSettings);
			AssetDatabase.SaveAssets();
        }
	}
}
