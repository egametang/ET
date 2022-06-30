using UnityEditor;

namespace YooAsset.Editor
{
	public static class AssetBundleSimulateBuilder
	{
		private static string _manifestFilePath = string.Empty;

		/// <summary>
		/// 模拟构建
		/// </summary>
		public static void SimulateBuild()
		{
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			BuildParameters buildParameters = new BuildParameters();
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
			buildParameters.BuildMode = EBuildMode.SimulateBuild;
			buildParameters.BuildVersion = 999;
			buildParameters.EnableAddressable = AssetBundleCollectorSettingData.Setting.EnableAddressable;

			AssetBundleBuilder builder = new AssetBundleBuilder();
			bool buildResult = builder.Run(buildParameters);
			if (buildResult)
			{
				string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(buildParameters.OutputRoot, buildParameters.BuildTarget);
				_manifestFilePath = $"{pipelineOutputDirectory}_{EBuildMode.SimulateBuild}/{YooAssetSettingsData.GetPatchManifestFileName(buildParameters.BuildVersion)}";
			}
			else
			{
				_manifestFilePath = null;
			}
		}
		
		/// <summary>
		/// 获取构建的补丁清单路径
		/// </summary>
		public static string GetPatchManifestPath()
		{
			return _manifestFilePath;
		}
	}
}