using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	[TaskAttribute("拷贝内置文件到流目录")]
	public class TaskCopyBuildinFiles : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<BuildParametersContext>();
			var manifestContext = context.GetContextObject<ManifestContext>();
			var buildMode = buildParametersContext.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				if (buildParametersContext.Parameters.CopyBuildinFileOption != ECopyBuildinFileOption.None)
				{
					CopyBuildinFilesToStreaming(buildParametersContext, manifestContext);
				}
			}
		}

		/// <summary>
		/// 拷贝首包资源文件
		/// </summary>
		private void CopyBuildinFilesToStreaming(BuildParametersContext buildParametersContext, ManifestContext manifestContext)
		{
			ECopyBuildinFileOption option = buildParametersContext.Parameters.CopyBuildinFileOption;
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			string streamingAssetsDirectory = AssetBundleBuilderHelper.GetStreamingAssetsFolderPath();
			string buildPackageName = buildParametersContext.Parameters.PackageName;
			string buildPackageVersion = buildParametersContext.Parameters.PackageVersion;

			// 加载补丁清单
			PackageManifest manifest = manifestContext.Manifest;

			// 清空流目录
			if (option == ECopyBuildinFileOption.ClearAndCopyAll || option == ECopyBuildinFileOption.ClearAndCopyByTags)
			{
				AssetBundleBuilderHelper.ClearStreamingAssetsFolder();
			}

			// 拷贝补丁清单文件
			{
				string fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildPackageName, buildPackageVersion);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单哈希文件
			{
				string fileName = YooAssetSettingsData.GetPackageHashFileName(buildPackageName, buildPackageVersion);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单版本文件
			{
				string fileName = YooAssetSettingsData.GetPackageVersionFileName(buildPackageName);
				string sourcePath = $"{packageOutputDirectory}/{fileName}";
				string destPath = $"{streamingAssetsDirectory}/{fileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝文件列表（所有文件）
			if (option == ECopyBuildinFileOption.ClearAndCopyAll || option == ECopyBuildinFileOption.OnlyCopyAll)
			{		
				foreach (var packageBundle in manifest.BundleList)
				{
					string sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
					string destPath = $"{streamingAssetsDirectory}/{packageBundle.FileName}";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}

			// 拷贝文件列表（带标签的文件）
			if (option == ECopyBuildinFileOption.ClearAndCopyByTags || option == ECopyBuildinFileOption.OnlyCopyByTags)
			{
				string[] tags = buildParametersContext.Parameters.CopyBuildinFileTags.Split(';');
				foreach (var packageBundle in manifest.BundleList)
				{
					if (packageBundle.HasTag(tags) == false)
						continue;
					string sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
					string destPath = $"{streamingAssetsDirectory}/{packageBundle.FileName}";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}

			// 刷新目录
			AssetDatabase.Refresh();
			BuildLogger.Log($"内置文件拷贝完成：{streamingAssetsDirectory}");
		}
	}
}