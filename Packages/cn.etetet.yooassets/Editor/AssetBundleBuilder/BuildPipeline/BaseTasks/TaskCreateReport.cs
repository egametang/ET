using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskCreateReport
    {
        protected void CreateReportFile(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext, ManifestContext manifestContext)
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

                // 收集器配置
                buildReport.Summary.UniqueBundleName = buildMapContext.Command.UniqueBundleName;
                buildReport.Summary.EnableAddressable = buildMapContext.Command.EnableAddressable;
                buildReport.Summary.LocationToLower = buildMapContext.Command.LocationToLower;
                buildReport.Summary.IncludeAssetGUID = buildMapContext.Command.IncludeAssetGUID;
                buildReport.Summary.IgnoreDefaultType = buildMapContext.Command.IgnoreDefaultType;
                buildReport.Summary.AutoCollectShaders = buildMapContext.Command.AutoCollectShaders;

                // 构建参数
                buildReport.Summary.EnableSharePackRule = buildParameters.EnableSharePackRule;
                buildReport.Summary.EncryptionClassName = buildParameters.EncryptionServices == null ? "null" : buildParameters.EncryptionServices.GetType().FullName;
                if (buildParameters.BuildPipeline == nameof(BuiltinBuildPipeline))
                {
                    var builtinBuildParameters = buildParameters as BuiltinBuildParameters;
                    buildReport.Summary.FileNameStyle = buildParameters.FileNameStyle;
                    buildReport.Summary.CompressOption = builtinBuildParameters.CompressOption;
                    buildReport.Summary.DisableWriteTypeTree = builtinBuildParameters.DisableWriteTypeTree;
                    buildReport.Summary.IgnoreTypeTreeChanges = builtinBuildParameters.IgnoreTypeTreeChanges;
                }
                else if (buildParameters.BuildPipeline == nameof(ScriptableBuildPipeline))
                {
                    var scriptableBuildParameters = buildParameters as ScriptableBuildParameters;
                    buildReport.Summary.FileNameStyle = buildParameters.FileNameStyle;
                    buildReport.Summary.CompressOption = scriptableBuildParameters.CompressOption;
                    buildReport.Summary.DisableWriteTypeTree = scriptableBuildParameters.DisableWriteTypeTree;
                    buildReport.Summary.IgnoreTypeTreeChanges = scriptableBuildParameters.IgnoreTypeTreeChanges;
                }
                else
                {
                    buildReport.Summary.FileNameStyle = buildParameters.FileNameStyle;
                    buildReport.Summary.CompressOption = ECompressOption.Uncompressed;
                    buildReport.Summary.DisableWriteTypeTree = false;
                    buildReport.Summary.IgnoreTypeTreeChanges = false;
                }

                // 构建结果
                buildReport.Summary.AssetFileTotalCount = buildMapContext.AssetFileCount;
                buildReport.Summary.MainAssetTotalCount = GetMainAssetCount(manifest);
                buildReport.Summary.AllBundleTotalCount = GetAllBundleCount(manifest);
                buildReport.Summary.AllBundleTotalSize = GetAllBundleSize(manifest);
                buildReport.Summary.EncryptedBundleTotalCount = GetEncryptedBundleCount(manifest);
                buildReport.Summary.EncryptedBundleTotalSize = GetEncryptedBundleSize(manifest);
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
                reportBundleInfo.Encrypted = packageBundle.Encrypted;
                reportBundleInfo.Tags = packageBundle.Tags;
                reportBundleInfo.DependBundles = GetDependBundles(manifest, packageBundle);
                reportBundleInfo.AllBuiltinAssets = GetAllBuiltinAssets(buildMapContext, packageBundle.BundleName);
                buildReport.BundleInfos.Add(reportBundleInfo);
            }

            // 其它资源列表
            buildReport.IndependAssets = new List<ReportIndependAsset>(buildMapContext.IndependAssets);

            // 序列化文件
            string fileName = YooAssetSettingsData.GetReportFileName(buildParameters.PackageName, buildParameters.PackageVersion);
            string filePath = $"{packageOutputDirectory}/{fileName}";
            BuildReport.Serialize(filePath, buildReport);
            BuildLogger.Log($"Create build report file: {filePath}");
        }

        /// <summary>
        /// 获取资源对象依赖的所有资源包
        /// </summary>
        private List<string> GetDependBundles(PackageManifest manifest, PackageBundle packageBundle)
        {
            List<string> dependBundles = new List<string>(packageBundle.DependIDs.Length);
            foreach (int index in packageBundle.DependIDs)
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
                foreach (var buildAsset in bundleInfo.MainAssets)
                {
                    if (buildAsset.AssetInfo.AssetPath == assetPath)
                    {
                        findAssetInfo = buildAsset;
                        break;
                    }
                }
                if (findAssetInfo == null)
                {
                    throw new Exception($"Should never get here ! Not found asset {assetPath} in bunlde {bundleName}");
                }
                foreach (var dependAssetInfo in findAssetInfo.AllDependAssetInfos)
                {
                    result.Add(dependAssetInfo.AssetInfo.AssetPath);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取该资源包内的所有资源
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
                if (packageBundle.Encrypted)
                    fileCount++;
            }
            return fileCount;
        }
        private long GetEncryptedBundleSize(PackageManifest manifest)
        {
            long fileBytes = 0;
            foreach (var packageBundle in manifest.BundleList)
            {
                if (packageBundle.Encrypted)
                    fileBytes += packageBundle.FileSize;
            }
            return fileBytes;
        }
    }
}