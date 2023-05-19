using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("创建构建报告文件")]
	public class TaskCreateReport : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var manifestContext = context.GetContextObject<ManifestContext>();

			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode != EBuildMode.SimulateBuild)
			{
				CreateReportFile(buildParameters, buildMapContext, manifestContext);
			}
		}

		private void CreateReportFile(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext, ManifestContext manifestContext)
		{
			var buildParameters = buildParametersContext.Parameters;

			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			PackageManifest manifest = manifestContext.Manifest;
			BuildReport buildReport = new BuildReport();

			// 概述信息
			{
#if UNITY_2019_4_OR_NEWER
				UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BuildReport).Assembly);
				if (packageInfo != null)
					buildReport.Summary.YooVersion = packageInfo.version;
#endif
				buildReport.Summary.UnityVersion = UnityEngine.Application.unityVersion;
				buildReport.Summary.BuildDate = DateTime.Now.ToString();
				buildReport.Summary.BuildSeconds = BuildRunner.TotalSeconds;
				buildReport.Summary.BuildTarget = buildParameters.BuildTarget;
				buildReport.Summary.BuildPipeline = buildParameters.BuildPipeline;
				buildReport.Summary.BuildMode = buildParameters.BuildMode;
				buildReport.Summary.BuildPackageName = buildParameters.PackageName;
				buildReport.Summary.BuildPackageVersion = buildParameters.PackageVersion;
				buildReport.Summary.EnableAddressable = buildMapContext.EnableAddressable;
				buildReport.Summary.UniqueBundleName = buildMapContext.UniqueBundleName;
				buildReport.Summary.EncryptionServicesClassName = buildParameters.EncryptionServices == null ?
					"null" : buildParameters.EncryptionServices.GetType().FullName;

				// 构建参数
				buildReport.Summary.OutputNameStyle = buildParameters.OutputNameStyle;
				buildReport.Summary.CompressOption = buildParameters.CompressOption;
				buildReport.Summary.DisableWriteTypeTree = buildParameters.DisableWriteTypeTree;
				buildReport.Summary.IgnoreTypeTreeChanges = buildParameters.IgnoreTypeTreeChanges;

				// 构建结果
				buildReport.Summary.AssetFileTotalCount = buildMapContext.AssetFileCount;
				buildReport.Summary.MainAssetTotalCount = GetMainAssetCount(manifest);
				buildReport.Summary.AllBundleTotalCount = GetAllBundleCount(manifest);
				buildReport.Summary.AllBundleTotalSize = GetAllBundleSize(manifest);
				buildReport.Summary.EncryptedBundleTotalCount = GetEncryptedBundleCount(manifest);
				buildReport.Summary.EncryptedBundleTotalSize = GetEncryptedBundleSize(manifest);
				buildReport.Summary.RawBundleTotalCount = GetRawBundleCount(manifest);
				buildReport.Summary.RawBundleTotalSize = GetRawBundleSize(manifest);
			}

			// 资源对象列表
			buildReport.AssetInfos = new List<ReportAssetInfo>(manifest.AssetList.Count);
			foreach (var packageAsset in manifest.AssetList)
			{
				var mainBundle = manifest.BundleList[packageAsset.BundleID];
				ReportAssetInfo reportAssetInfo = new ReportAssetInfo();
				reportAssetInfo.Address = packageAsset.Address;
				reportAssetInfo.AssetPath = packageAsset.AssetPath;
				reportAssetInfo.AssetTags = packageAsset.AssetTags;
				reportAssetInfo.AssetGUID = AssetDatabase.AssetPathToGUID(packageAsset.AssetPath);
				reportAssetInfo.MainBundleName = mainBundle.BundleName;
				reportAssetInfo.MainBundleSize = mainBundle.FileSize;
				reportAssetInfo.DependBundles = GetDependBundles(manifest, packageAsset);
				reportAssetInfo.DependAssets = GetDependAssets(buildMapContext, mainBundle.BundleName, packageAsset.AssetPath);
				buildReport.AssetInfos.Add(reportAssetInfo);
			}

			// 资源包列表
			buildReport.BundleInfos = new List<ReportBundleInfo>(manifest.BundleList.Count);
			foreach (var packageBundle in manifest.BundleList)
			{
				ReportBundleInfo reportBundleInfo = new ReportBundleInfo();
				reportBundleInfo.BundleName = packageBundle.BundleName;
				reportBundleInfo.FileName = packageBundle.FileName;
				reportBundleInfo.FileHash = packageBundle.FileHash;
				reportBundleInfo.FileCRC = packageBundle.FileCRC;
				reportBundleInfo.FileSize = packageBundle.FileSize;
				reportBundleInfo.IsRawFile = packageBundle.IsRawFile;
				reportBundleInfo.LoadMethod = (EBundleLoadMethod)packageBundle.LoadMethod;
				reportBundleInfo.Tags = packageBundle.Tags;
				reportBundleInfo.ReferenceIDs = packageBundle.ReferenceIDs;
				reportBundleInfo.AllBuiltinAssets = GetAllBuiltinAssets(buildMapContext, packageBundle.BundleName);
				buildReport.BundleInfos.Add(reportBundleInfo);
			}

			// 序列化文件
			string fileName = YooAssetSettingsData.GetReportFileName(buildParameters.PackageName, buildParameters.PackageVersion);
			string filePath = $"{packageOutputDirectory}/{fileName}";
			BuildReport.Serialize(filePath, buildReport);
			BuildLogger.Log($"资源构建报告文件创建完成：{filePath}");
		}

		/// <summary>
		/// 获取资源对象依赖的所有资源包
		/// </summary>
		private List<string> GetDependBundles(PackageManifest manifest, PackageAsset packageAsset)
		{
			List<string> dependBundles = new List<string>(packageAsset.DependIDs.Length);
			foreach (int index in packageAsset.DependIDs)
			{
				string dependBundleName = manifest.BundleList[index].BundleName;
				dependBundles.Add(dependBundleName);
			}
			return dependBundles;
		}

		/// <summary>
		/// 获取资源对象依赖的其它所有资源
		/// </summary>
		private List<string> GetDependAssets(BuildMapContext buildMapContext, string bundleName, string assetPath)
		{
			List<string> result = new List<string>();
			var bundleInfo = buildMapContext.GetBundleInfo(bundleName);
			{
				BuildAssetInfo findAssetInfo = null;
				foreach (var assetInfo in bundleInfo.AllMainAssets)
				{
					if (assetInfo.AssetPath == assetPath)
					{
						findAssetInfo = assetInfo;
						break;
					}
				}
				if (findAssetInfo == null)
				{
					throw new Exception($"Not found asset {assetPath} in bunlde {bundleName}");
				}
				foreach (var dependAssetInfo in findAssetInfo.AllDependAssetInfos)
				{
					result.Add(dependAssetInfo.AssetPath);
				}
			}
			return result;
		}

		/// <summary>
		/// 获取该资源包内的所有资源（包括零依赖资源）
		/// </summary>
		private List<string> GetAllBuiltinAssets(BuildMapContext buildMapContext, string bundleName)
		{
			var bundleInfo = buildMapContext.GetBundleInfo(bundleName);
			return bundleInfo.GetAllBuiltinAssetPaths();
		}

		private int GetMainAssetCount(PackageManifest manifest)
		{
			return manifest.AssetList.Count;
		}
		private int GetAllBundleCount(PackageManifest manifest)
		{
			return manifest.BundleList.Count;
		}
		private long GetAllBundleSize(PackageManifest manifest)
		{
			long fileBytes = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				fileBytes += packageBundle.FileSize;
			}
			return fileBytes;
		}
		private int GetEncryptedBundleCount(PackageManifest manifest)
		{
			int fileCount = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				if (packageBundle.LoadMethod != (byte)EBundleLoadMethod.Normal)
					fileCount++;
			}
			return fileCount;
		}
		private long GetEncryptedBundleSize(PackageManifest manifest)
		{
			long fileBytes = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				if (packageBundle.LoadMethod != (byte)EBundleLoadMethod.Normal)
					fileBytes += packageBundle.FileSize;
			}
			return fileBytes;
		}
		private int GetRawBundleCount(PackageManifest manifest)
		{
			int fileCount = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				if (packageBundle.IsRawFile)
					fileCount++;
			}
			return fileCount;
		}
		private long GetRawBundleSize(PackageManifest manifest)
		{
			long fileBytes = 0;
			foreach (var packageBundle in manifest.BundleList)
			{
				if (packageBundle.IsRawFile)
					fileBytes += packageBundle.FileSize;
			}
			return fileBytes;
		}
	}
}