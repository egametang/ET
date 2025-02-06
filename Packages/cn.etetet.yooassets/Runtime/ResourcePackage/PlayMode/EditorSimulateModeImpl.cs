using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class EditorSimulateModeImpl : IPlayMode, IBundleQuery
    {
        private PackageManifest _activeManifest;
        private ResourceAssist _assist;

        public readonly string PackageName;
        public DownloadManager Download
        {
            get { return _assist.Download; }
        }

        public EditorSimulateModeImpl(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(ResourceAssist assist, string simulateManifestFilePath)
        {
            _assist = assist;

            var operation = new EditorSimulateModeInitializationOperation(this, simulateManifestFilePath);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        #region IPlayMode接口
        public PackageManifest ActiveManifest
        {
            set
            {
                _activeManifest = value;
            }
            get
            {
                return _activeManifest;
            }
        }
        public void FlushManifestVersionFile()
        {
        }

        UpdatePackageVersionOperation IPlayMode.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new EditorPlayModeUpdatePackageVersionOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        UpdatePackageManifestOperation IPlayMode.UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout)
        {
            var operation = new EditorPlayModeUpdatePackageManifestOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        PreDownloadContentOperation IPlayMode.PreDownloadContentAsync(string packageVersion, int timeout)
        {
            var operation = new EditorPlayModePreDownloadContentOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(Download, PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(Download, PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(Download, PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }

        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceUnpackerOperation.CreateEmptyUnpacker(Download, PackageName, upackingMaxNumber, failedTryAgain, timeout);
        }
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceUnpackerOperation.CreateEmptyUnpacker(Download, PackageName, upackingMaxNumber, failedTryAgain, timeout);
        }

        ResourceImporterOperation IPlayMode.CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceImporterOperation.CreateEmptyImporter(Download, PackageName, importerMaxNumber, failedTryAgain, timeout);
        }
        #endregion

        #region IBundleQuery接口
        private BundleInfo CreateBundleInfo(PackageBundle packageBundle, AssetInfo assetInfo)
        {
            if (packageBundle == null)
                throw new Exception("Should never get here !");

            BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromEditor);
            bundleInfo.IncludeAssetsInEditor = _activeManifest.GetBundleIncludeAssets(assetInfo.AssetPath);
            return bundleInfo;
        }
        BundleInfo IBundleQuery.GetMainBundleInfo(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = _activeManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return CreateBundleInfo(packageBundle, assetInfo);
        }
        BundleInfo[] IBundleQuery.GetDependBundleInfos(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = _activeManifest.GetAllDependencies(assetInfo.AssetPath);
            List<BundleInfo> result = new List<BundleInfo>(depends.Length);
            foreach (var packageBundle in depends)
            {
                BundleInfo bundleInfo = CreateBundleInfo(packageBundle, assetInfo);
                result.Add(bundleInfo);
            }
            return result.ToArray();
        }
        string IBundleQuery.GetMainBundleName(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = _activeManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return packageBundle.BundleName;
        }
        string[] IBundleQuery.GetDependBundleNames(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = _activeManifest.GetAllDependencies(assetInfo.AssetPath);
            List<string> result = new List<string>(depends.Length);
            foreach (var packageBundle in depends)
            {
                result.Add(packageBundle.BundleName);
            }
            return result.ToArray();
        }
        bool IBundleQuery.ManifestValid()
        {
            return _activeManifest != null;
        }
        #endregion
    }
}