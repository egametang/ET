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
			// 注意：我们只有在强制重建的时候才会拷贝
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			if (buildParameters.Parameters.CopyBuildinTagFiles)
			{
				// 清空流目录
				AssetBundleBuilderHelper.ClearStreamingAssetsFolder();

				// 拷贝内置文件
				CopyBuildinFilesToStreaming(buildParameters.PipelineOutputDirectory, buildParameters.Parameters.BuildVersion);
			}
		}

		private void CopyBuildinFilesToStreaming(string pipelineOutputDirectory, int buildVersion)
		{
			// 加载补丁清单
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(pipelineOutputDirectory, buildVersion);

			// 拷贝文件列表
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin == false)
					continue;

				string sourcePath = $"{pipelineOutputDirectory}/{patchBundle.BundleName}";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{patchBundle.Hash}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝清单文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestFileName(buildVersion)}";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{YooAssetSettingsData.GetPatchManifestFileName(buildVersion)}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝清单哈希文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestHashFileName(buildVersion)}";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{YooAssetSettingsData.GetPatchManifestHashFileName(buildVersion)}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝静态版本文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettings.VersionFileName}";
				string destPath = $"{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}/{YooAssetSettings.VersionFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 刷新目录
			AssetDatabase.Refresh();
			BuildRunner.Log($"内置文件拷贝完成：{AssetBundleBuilderHelper.GetStreamingAssetsFolderPath()}");
		}
	}
}