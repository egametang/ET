using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace ET
{
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
		private PlatformType activePlatform;
		private PlatformType platformType;
		private bool clearFolder;
		private bool isBuildExe;
		private bool isContainAB;
		private CodeOptimization codeOptimization = CodeOptimization.Debug;
		private BuildOptions buildOptions;
		private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;

		private GlobalConfig globalConfig;

		[MenuItem("ET/Build Tool")]
		public static void ShowWindow()
		{
			GetWindow(typeof (BuildEditor));
		}

        private void OnEnable()
		{
			globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
			
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
        }

        private void OnGUI() 
		{
			EditorGUILayout.LabelField("打包平台:");
			this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
			this.clearFolder = EditorGUILayout.Toggle("清理资源文件夹: ", clearFolder);
			this.isBuildExe = EditorGUILayout.Toggle("是否打包EXE: ", this.isBuildExe);
			this.isContainAB = EditorGUILayout.Toggle("是否同将资源打进EXE: ", this.isContainAB);
			this.codeOptimization = (CodeOptimization)EditorGUILayout.EnumPopup("CodeOptimization: ", this.codeOptimization);
			EditorGUILayout.LabelField("BuildAssetBundleOptions(可多选):");
			this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);
			
			switch (this.codeOptimization)
			{
				case CodeOptimization.None:
				case CodeOptimization.Debug:
					this.buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler;
					break;
				case CodeOptimization.Release:
					this.buildOptions = BuildOptions.None;
					break;
			}

			GUILayout.Space(5);
			
			if (GUILayout.Button("BuildPackage"))
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
			
			GUILayout.Label("");
			GUILayout.Label("代码编译：");
			
			this.globalConfig.LoadMode = (LoadMode)EditorGUILayout.EnumPopup("LoadMode: ", this.globalConfig.LoadMode);
			
			this.globalConfig.CodeMode = (CodeMode)EditorGUILayout.EnumPopup("CodeMode: ", this.globalConfig.CodeMode);
			
			if (GUILayout.Button("BuildCode"))
			{
				BuildAssemblieEditor.BuildCode(this.codeOptimization, globalConfig);
			}
			
			if (GUILayout.Button("BuildModel"))
			{
				BuildAssemblieEditor.BuildModel(this.codeOptimization, globalConfig);
			}
			
			if (GUILayout.Button("BuildHotfix"))
			{
				BuildAssemblieEditor.BuildHotfix(this.codeOptimization, globalConfig);
			}
			
			if (GUILayout.Button("ExcelExporter"))
			{
				//Directory.Delete("Assets/Bundles/Config", true);
				ToolsEditor.ExcelExporter();
				// 如果是ClientServer，那么客户端要使用服务端配置
				if (this.globalConfig.CodeMode == CodeMode.ClientServer)
				{
					FileHelper.CopyDirectory("../Config/StartConfig/Localhost", "Assets/Bundles/Config/StartConfig/Localhost");
					foreach (string file in Directory.GetFiles("../Config/", "*.bytes"))
					{
						File.Copy(file, $"Assets/Bundles/Config/{Path.GetFileName(file)}", true);
					}
				}
				Debug.Log("copy config to Assets/Bundles/Config");
			}
			
			if (GUILayout.Button("Proto2CS"))
			{
				ToolsEditor.Proto2CS();
			}
			


			GUILayout.Space(5);
		}
	}
}
