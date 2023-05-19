using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("更新资源包信息")]
	public class TaskUpdateBundleInfo : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			int outputNameStyle = (int)buildParametersContext.Parameters.OutputNameStyle;

			// 1.检测文件名长度
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				// NOTE：检测文件名长度不要超过260字符。
				string fileName = bundleInfo.BundleName;
				if (fileName.Length >= 260)
					throw new Exception($"The output bundle name is too long {fileName.Length} chars : {fileName}");
			}

			// 2.更新构建输出的文件路径
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				if (bundleInfo.IsEncryptedFile)
					bundleInfo.BundleInfo.BuildOutputFilePath = bundleInfo.EncryptedFilePath;
				else
					bundleInfo.BundleInfo.BuildOutputFilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
			}

			// 3.更新文件其它信息
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				string buildOutputFilePath = bundleInfo.BundleInfo.BuildOutputFilePath;
				bundleInfo.BundleInfo.ContentHash = GetBundleContentHash(bundleInfo, context);
				bundleInfo.BundleInfo.FileHash = GetBundleFileHash(buildOutputFilePath, buildParametersContext);
				bundleInfo.BundleInfo.FileCRC = GetBundleFileCRC(buildOutputFilePath, buildParametersContext);
				bundleInfo.BundleInfo.FileSize = GetBundleFileSize(buildOutputFilePath, buildParametersContext);
			}

			// 4.更新补丁包输出的文件路径
			foreach (var bundleInfo in buildMapContext.Collection)
			{
				string fileExtension = ManifestTools.GetRemoteBundleFileExtension(bundleInfo.BundleName);
				string fileName = ManifestTools.GetRemoteBundleFileName(outputNameStyle, bundleInfo.BundleName, fileExtension, bundleInfo.BundleInfo.FileHash);
				bundleInfo.BundleInfo.PackageOutputFilePath = $"{packageOutputDirectory}/{fileName}";
			}
		}

		private string GetBundleContentHash(BuildBundleInfo bundleInfo, BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var parameters = buildParametersContext.Parameters;
			var buildMode = parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000000000000000000000000000"; //32位

			if (bundleInfo.IsRawFile)
			{
				string filePath = bundleInfo.BundleInfo.BuildOutputFilePath;
				return HashUtility.FileMD5(filePath);
			}

			if (parameters.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				var buildResult = context.GetContextObject<TaskBuilding.BuildResultContext>();
				var hash = buildResult.UnityManifest.GetAssetBundleHash(bundleInfo.BundleName);
				if (hash.isValid)
					return hash.ToString();
				else
					throw new Exception($"Not found bundle in build result : {bundleInfo.BundleName}");
			}
			else if (parameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				// 注意：当资源包的依赖列表发生变化的时候，ContentHash也会发生变化！
				var buildResult = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
				if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
					return value.Hash.ToString();
				else
					throw new Exception($"Not found bundle in build result : {bundleInfo.BundleName}");
			}
			else
			{
				throw new System.NotImplementedException();
			}
		}
		private string GetBundleFileHash(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000000000000000000000000000"; //32位
			else
				return HashUtility.FileMD5(filePath);
		}
		private string GetBundleFileCRC(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return "00000000"; //8位
			else
				return HashUtility.FileCRC32(filePath);
		}
		private long GetBundleFileSize(string filePath, BuildParametersContext buildParametersContext)
		{
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.DryRunBuild || buildMode == EBuildMode.SimulateBuild)
				return 0;
			else
				return FileUtility.GetFileSize(filePath);
		}
	}
}