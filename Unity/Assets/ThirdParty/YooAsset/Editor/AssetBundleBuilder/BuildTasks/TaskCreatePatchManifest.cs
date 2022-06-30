using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("创建补丁清单文件")]
	public class TaskCreatePatchManifest : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var encryptionContext = context.GetContextObject<TaskEncryption.EncryptionContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			CreatePatchManifestFile(buildParameters, buildMapContext, encryptionContext);
		}

		/// <summary>
		/// 创建补丁清单文件到输出目录
		/// </summary>
		private void CreatePatchManifestFile(AssetBundleBuilder.BuildParametersContext buildParameters,
			BuildMapContext buildMapContext, TaskEncryption.EncryptionContext encryptionContext)
		{
			int resourceVersion = buildParameters.Parameters.BuildVersion;

			// 创建新补丁清单
			PatchManifest patchManifest = new PatchManifest();
			patchManifest.EnableAddressable = buildParameters.Parameters.EnableAddressable;
			patchManifest.ResourceVersion = buildParameters.Parameters.BuildVersion;
			patchManifest.BuildinTags = buildParameters.Parameters.BuildinTags;
			patchManifest.BundleList = GetAllPatchBundle(buildParameters, buildMapContext, encryptionContext);
			patchManifest.AssetList = GetAllPatchAsset(buildParameters, buildMapContext, patchManifest);

			// 创建补丁清单文件
			string manifestFilePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestFileName(resourceVersion)}";
			BuildRunner.Log($"创建补丁清单文件：{manifestFilePath}");
			PatchManifest.Serialize(manifestFilePath, patchManifest);

			// 创建补丁清单哈希文件
			string manifestHashFilePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestHashFileName(resourceVersion)}";
			string manifestHash = HashUtility.FileMD5(manifestFilePath);
			BuildRunner.Log($"创建补丁清单哈希文件：{manifestHashFilePath}");
			FileUtility.CreateFile(manifestHashFilePath, manifestHash);

			// 创建静态版本文件
			string staticVersionFilePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.VersionFileName}";
			string staticVersion = resourceVersion.ToString();
			BuildRunner.Log($"创建静态版本文件：{staticVersionFilePath}");
			FileUtility.CreateFile(staticVersionFilePath, staticVersion);
		}

		/// <summary>
		/// 获取资源包列表
		/// </summary>
		private List<PatchBundle> GetAllPatchBundle(AssetBundleBuilder.BuildParametersContext buildParameters,
			BuildMapContext buildMapContext, TaskEncryption.EncryptionContext encryptionContext)
		{
			List<PatchBundle> result = new List<PatchBundle>(1000);

			List<string> buildinTags = buildParameters.Parameters.GetBuildinTags();
			var buildMode = buildParameters.Parameters.BuildMode;
			bool standardBuild = buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild;
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var bundleName = bundleInfo.BundleName;
				string filePath = $"{buildParameters.PipelineOutputDirectory}/{bundleName}";
				string hash = GetFileHash(filePath, standardBuild);
				string crc32 = GetFileCRC(filePath, standardBuild);
				long size = GetFileSize(filePath, standardBuild);
				string[] tags = buildMapContext.GetBundleTags(bundleName);
				bool isEncrypted = encryptionContext.IsEncryptFile(bundleName);
				bool isBuildin = IsBuildinBundle(tags, buildinTags);
				bool isRawFile = bundleInfo.IsRawFile;

				// 附加文件扩展名
				if (buildParameters.Parameters.AppendFileExtension)
				{
					hash += bundleInfo.GetAppendExtension();
				}

				PatchBundle patchBundle = new PatchBundle(bundleName, hash, crc32, size, tags);
				patchBundle.SetFlagsValue(isEncrypted, isBuildin, isRawFile);
				result.Add(patchBundle);
			}

			return result;
		}
		private bool IsBuildinBundle(string[] bundleTags, List<string> buildinTags)
		{
			// 注意：没有任何分类标签的Bundle文件默认为内置文件
			if (bundleTags.Length == 0)
				return true;

			foreach (var tag in bundleTags)
			{
				if (buildinTags.Contains(tag))
					return true;
			}
			return false;
		}
		private string GetFileHash(string filePath, bool standardBuild)
		{
			if (standardBuild)
				return HashUtility.FileMD5(filePath);
			else
				return "00000000000000000000000000000000"; //32位
		}
		private string GetFileCRC(string filePath, bool standardBuild)
		{
			if (standardBuild)
				return HashUtility.FileCRC32(filePath);
			else
				return "00000000"; //8位
		}
		private long GetFileSize(string filePath, bool standardBuild)
		{
			if (standardBuild)
				return FileUtility.GetFileSize(filePath);
			else
				return 0;
		}

		/// <summary>
		/// 获取资源列表
		/// </summary>
		private List<PatchAsset> GetAllPatchAsset(AssetBundleBuilder.BuildParametersContext buildParameters,
			BuildMapContext buildMapContext, PatchManifest patchManifest)
		{
			List<PatchAsset> result = new List<PatchAsset>(1000);
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				var assetInfos = bundleInfo.GetAllPatchAssetInfos();
				foreach (var assetInfo in assetInfos)
				{
					PatchAsset patchAsset = new PatchAsset();
					if (buildParameters.Parameters.EnableAddressable)
						patchAsset.Address = assetInfo.Address;
					else
						patchAsset.Address = string.Empty;
					patchAsset.AssetPath = assetInfo.AssetPath;
					patchAsset.AssetTags = assetInfo.AssetTags.ToArray();
					patchAsset.BundleID = GetAssetBundleID(assetInfo.GetBundleName(), patchManifest);
					patchAsset.DependIDs = GetAssetBundleDependIDs(patchAsset.BundleID, assetInfo, patchManifest);
					result.Add(patchAsset);
				}
			}
			return result;
		}
		private int[] GetAssetBundleDependIDs(int mainBundleID, BuildAssetInfo assetInfo, PatchManifest patchManifest)
		{
			List<int> result = new List<int>();
			foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
			{
				if (dependAssetInfo.HasBundleName())
				{
					int bundleID = GetAssetBundleID(dependAssetInfo.GetBundleName(), patchManifest);
					if (mainBundleID != bundleID)
					{
						if (result.Contains(bundleID) == false)
							result.Add(bundleID);
					}
				}
			}
			return result.ToArray();
		}
		private int GetAssetBundleID(string bundleName, PatchManifest patchManifest)
		{
			for (int index = 0; index < patchManifest.BundleList.Count; index++)
			{
				if (patchManifest.BundleList[index].BundleName == bundleName)
					return index;
			}
			throw new Exception($"Not found bundle name : {bundleName}");
		}
	}
}